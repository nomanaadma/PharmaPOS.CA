using ErrorOr;
using Microsoft.EntityFrameworkCore;
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
	IRepository<Purchase> purchaseRepo,
	IRepository<Supplier> supplierRepo,
	IRepository<Batch> batchRepo,
	IUnitOfWork unitOfWork,
	MedicineService medicineService,
	BatchService batchService,
	ILogger<PurchaseService> logger) : IApplicationService
{
	public async Task<DataTableResponse<PurchaseListResponse>> GetFilteredPurchasesAsync(DataTableRequest request, DataTableColumnMap<PurchaseListResponse> columns, CancellationToken ct = default)
	{
		var query = purchaseRepo.Query()
			.AsNoTracking()
			.Join(supplierRepo.Query(), p => p.SupplierId, s => s.Id, (p, s) => new PurchaseListResponse
			{
				Id = p.Id,
				Date = p.Date,
				SubTotal = p.SubTotal,
				Tax = p.Tax,
				Discount = p.Discount,
				Amount = p.Amount,
				Note = p.Note,
				SupplierName = s.Name
			});

		return await DataTableHelper.FilterAsync(query, request, columns, ct);
	}

	public async Task<ErrorOr<int>> CreatePurchaseAsync(PurchaseRequest request, CancellationToken ct = default)
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
			return purchase.Id;
		}
		catch
		{
			await transaction.RollbackAsync(ct);
			throw;
		}
	}

	public async Task<ErrorOr<int>> UpdatePurchaseAsync(int id, PurchaseRequest request, CancellationToken ct = default)
	{
		if (request.Items.Count == 0)
			return PurchaseErrors.MissingItems;
		
		var existingPurchase = await purchaseRepo.FirstOrDefaultAsync(mp => mp.Id == id, ct);

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

		var existingBatches = await batchService.GetBatchesByPurchaseIdAsync(id, ct);
		
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
		return existingPurchase.Id;
	}

	public async Task<ErrorOr<Deleted>> DeletePurchaseAsync(int id, CancellationToken ct = default)
	{
		var purchase = await purchaseRepo.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (purchase == null)
			return PurchaseErrors.NotFound(id);
		
		var hasSoldBatches = await batchService.HasSoldBatchesAsync(id, ct);
		
		if (hasSoldBatches)
			return BatchErrors.CannotDeletePurchaseWithSoldBatches;
		
		await batchService.DeleteBatchesByPurchaseIdAsync(id, ct);
		
		purchaseRepo.Remove(purchase);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Purchase {PurchaseId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<PurchaseDetailsResponse>> GetPurchaseAsync(int id, CancellationToken ct = default)
	{
		var purchase = await purchaseRepo.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (purchase == null)
			return PurchaseErrors.NotFound(id);
		
		var supplier = await supplierRepo.FirstOrDefaultAsync(s => s.Id == purchase.SupplierId, ct);
		if (supplier == null)
			return PurchaseErrors.SupplierNotFound(id);

		// Fetch batches separately (respects aggregate boundary)
		var batches = await batchService.GetBatchesByPurchaseIdAsync(id, ct);
		
		// Fetch medicine names for display
		var medicineIds = batches.Select(b => b.MedicineId).Distinct().ToList();
		
		var medicineNames = await medicineService.GetMedicineNamesByIds(medicineIds, ct);
		
		return new PurchaseDetailsResponse(purchase, batches, medicineNames, supplier);
	}
}
