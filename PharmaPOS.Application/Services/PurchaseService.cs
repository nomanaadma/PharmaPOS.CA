using ErrorOr;
using Mapster;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Helpers;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Application.Services;

public class PurchaseService(
	IPurchaseRepository purchaseRepo,
	IBatchRepository batchRepo,
	IMedicineRepository medicineRepo,
	ISupplierRepository supplierRepo,
	IUnitOfWork unitOfWork,
	ILogger<PurchaseService> logger) : IApplicationService
{
	public async Task<DataTableResponse<PurchaseListResponse>> GetFilteredPurchasesAsync(
		DataTableRequest request, 
		DataTableColumnMap<PurchaseListResponse> columns, 
		CancellationToken ct = default)
	{
		return await purchaseRepo.GetFilteredPurchasesAsync(request, columns, ct);
	}

	public async Task<ErrorOr<Created>> CreatePurchaseAsync(PurchaseRequest request, CancellationToken ct = default)
	{
		if (request.Items.Count == 0)
			return PurchaseErrors.MissingItems;

		var details = new PurchaseDetails(
			request.Supplier,
			request.Date,
			request.SubTotal,
			request.Tax,
			request.Discount,
			request.Amount,
			request.Note
		);

		var purchaseResult = Purchase.Create(details);
		if (purchaseResult.IsError)
			return purchaseResult.Errors;

		var purchase = purchaseResult.Value;
		
		await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
		
		try
		{
			await purchaseRepo.AddAsync(purchase, ct);
			await unitOfWork.SaveChangesAsync(ct);

			var batches = new List<Batch>();

			foreach (var item in request.Items)
			{
				var pricingResult = PricingHelper.CreatePricingDetails(item);
				if (pricingResult.IsError) return pricingResult.Errors;

				var batchDetails = new BatchDetails(
					item.Batch,
					item.Expiry,
					item.Pqty,
					pricingResult.Value,
					item.Purchaseprice,
					item.MedicineId,
					purchase.Id
				);

				var batchResult = Batch.Create(batchDetails);
			
				if (batchResult.IsError)
					return batchResult.Errors;
			
				batches.Add(batchResult.Value);
			}

			await batchRepo.AddRangeAsync(batches, ct);
			await unitOfWork.SaveChangesAsync(ct);
			
			await transaction.CommitAsync(ct);
			
			logger.LogInformation("Purchase {PurchaseId} created with {BatchCount} batches", purchase.Id, request.Items.Count);
			return Result.Created;
		}
		catch
		{
			await transaction.RollbackAsync(ct);
			throw;
		}
	}

	public async Task<ErrorOr<Updated>> UpdatePurchaseAsync(int id, PurchaseRequest request, CancellationToken ct = default)
	{
		if (request.Items.Count == 0)
			return PurchaseErrors.MissingItems;
		
		var existingPurchase = await purchaseRepo.GetByIdAsync(id, ct);

		if (existingPurchase == null)
			return PurchaseErrors.NotFound(id);

		var details = new PurchaseDetails(
			request.Supplier,
			request.Date,
			request.SubTotal,
			request.Tax,
			request.Discount,
			request.Amount,
			request.Note
		);

		var updateResult = existingPurchase.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		var submittedBatchIds = request.Items.Where(i => i.Id != 0).Select(i => i.Id).ToHashSet();

		var existingBatches = await batchRepo.GetBatchesByPurchaseIdAsync(id, ct);
		
		var batchesToRemove = existingBatches.Where(b => !submittedBatchIds.Contains(b.Id)).ToList();
		
		foreach (var batch in batchesToRemove)
		{
			// Business rule: can't delete a batch that has been sold
			if (batch.Sold > 0)
				return BatchErrors.CannotDeleteSoldBatch(batch.BatchName);
		}
		
		batchRepo.RemoveRange(batchesToRemove);
		
		var batches = new List<Batch>();

		foreach (var item in request.Items)
		{
			var pricingResult = PricingHelper.CreatePricingDetails(item);
			if (pricingResult.IsError) return pricingResult.Errors;

			var batchDetails = new BatchDetails(
				item.Batch,
				item.Expiry,
				item.Pqty,
				pricingResult.Value,
				item.Purchaseprice,
				item.MedicineId,
				id
			);

			if (item.Id != 0)
			{
				var existingBatch = existingBatches.FirstOrDefault(b => b.Id == item.Id);

				if (existingBatch == null) continue;

				if (pricingResult.Value.Qty < existingBatch.Sold)
					return BatchErrors.CannotUpdateSoldQuantity(existingBatch.BatchName, pricingResult.Value.Qty, existingBatch.Sold);

				var batchUpdateResult = existingBatch.Update(batchDetails);
				if (batchUpdateResult.IsError)
					return batchUpdateResult.Errors;
			}
			else
			{
				var batchResult = Batch.Create(batchDetails);
				if (batchResult.IsError)
					return batchResult.Errors;
				
				batches.Add(batchResult.Value);
			}
		}

		if (batches.Count != 0)
		{
			await batchRepo.AddRangeAsync(batches, ct);
		}

		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Purchase {PurchaseId} updated with {BatchCount} batches", existingPurchase.Id, request.Items.Count);
		return Result.Updated;
	}

	public async Task<ErrorOr<Deleted>> DeletePurchaseAsync(int id, CancellationToken ct = default)
	{
		var purchase = await purchaseRepo.GetByIdAsync(id, ct);

		if (purchase == null)
			return PurchaseErrors.NotFound(id);
		
		var hasSoldBatches = await batchRepo.HasSoldBatchesAsync(id, ct);
		
		if (hasSoldBatches)
			return BatchErrors.CannotDeletePurchaseWithSoldBatches;
		
		await batchRepo.DeleteBatchesByPurchaseIdAsync(id, ct);
		
		purchaseRepo.Remove(purchase);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Purchase {PurchaseId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<PurchaseResponse>> GetPurchaseAsync(int id, CancellationToken ct = default)
	{
		
		var purchase = await purchaseRepo.GetByIdAsync(id, ct);
		if (purchase == null)
			return PurchaseErrors.NotFound(id);
		
		var supplier = await supplierRepo.GetByIdAsync(purchase.SupplierId, ct);
		if(supplier == null)
			return SupplierErrors.NotFound(id);
		
		var batches = await batchRepo.GetBatchesByPurchaseIdAsync(id, ct);

		var purchaseItems = batches.Adapt<List<PurchaseItemResponse>>(); 

		var medicineIds = purchaseItems.Select(b => b.MedicineId).Distinct().ToList();

		var medicineNames = await medicineRepo.GetMedicineNamesByIdsAsync(medicineIds, ct);

		foreach (var purchaseItem in purchaseItems)
		{
			var name = medicineNames.GetValueOrDefault(purchaseItem.MedicineId, "");
			purchaseItem.Name = name;
		}

		var purchaseResponse = purchase.Adapt<PurchaseResponse>() with
		{
			Items = purchaseItems,
			SupplierName = supplier.Name,
			SupplierEmail = supplier.Email,
			SupplierPhone = supplier.Phone,
			Supplier = supplier.Id
		};

		return purchaseResponse;
		
	}
}
