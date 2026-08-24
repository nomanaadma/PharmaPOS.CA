using ErrorOr;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Helpers;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class PurchaseService(
	ApplicationDbContext context,
	ILogger<PurchaseService> logger)
{
	public async Task<ErrorOr<int>> CreatePurchaseAsync(PurchaseFormModel model, CancellationToken ct = default)
	{
		if (model.Items.Count == 0)
			return PurchaseErrors.MissingItems;

		await using var transaction = await context.Database.BeginTransactionAsync(ct);

		var details = new PurchaseDetails(
			model.Supplier,
			model.Date,
			model.SubTotal,
			model.Tax,
			model.Discount,
			model.Amount,
			model.Note
		);

		var purchaseResult = Purchase.Create(details);
		if (purchaseResult.IsError)
			return purchaseResult.Errors;

		var purchase = purchaseResult.Value;
		context.Purchases.Add(purchase);
		await context.SaveChangesAsync(ct);

		foreach (var item in model.Items)
		{
			var pricing = PricingHelper.PricingDetails(item);

			var batchDetails = new BatchDetails(
				item.Batch,
				item.Expiry,
				item.Pqty,
				pricing,
				item.Purchaseprice,
				item.MedicineId,
				purchase.Id
			);

			var batchResult = Batch.Create(batchDetails);
			if (batchResult.IsError)
				return batchResult.Errors;

			purchase.AddBatch(batchResult.Value);
		}

		await context.SaveChangesAsync(ct);
		await transaction.CommitAsync(ct);

		logger.LogInformation("Purchase {PurchaseId} created with {BatchCount} batches", purchase.Id, model.Items.Count);
		return purchase.Id;
	}

	public async Task<ErrorOr<int>> UpdatePurchaseAsync(int id, PurchaseFormModel model, CancellationToken ct = default)
	{
		if (model.Items.Count == 0)
			return PurchaseErrors.MissingItems;

		await using var transaction = await context.Database.BeginTransactionAsync(ct);

		var existingPurchase = await context.Purchases
			.Include(mp => mp.Batches)
			.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (existingPurchase == null)
			return PurchaseErrors.NotFound(id);

		var details = new PurchaseDetails(
			model.Supplier,
			model.Date,
			model.SubTotal,
			model.Tax,
			model.Discount,
			model.Amount,
			model.Note
		);

		var updateResult = existingPurchase.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		var submittedBatchIds = model.Items.Where(i => i.Id != 0).Select(i => i.Id).ToList();

		var batchesToRemoveIds = existingPurchase.Batches.Where(b => !submittedBatchIds.Contains(b.Id)).Select(b => b.Id).ToList();
		
		foreach (var batchId in batchesToRemoveIds)
		{
			// Delegate the removal and rule validation to the aggregate
			var removeResult = existingPurchase.RemoveBatch(batchId);
    
			if (removeResult.IsError)
				return removeResult.Errors;
		}

		foreach (var item in model.Items)
		{
			var pricing = PricingHelper.PricingDetails(item);

			var batchDetails = new BatchDetails(
				item.Batch,
				item.Expiry,
				item.Pqty,
				pricing,
				item.Purchaseprice,
				item.MedicineId,
				existingPurchase.Id
			);

			if (item.Id != 0)
			{
				var existingBatch = existingPurchase.Batches.FirstOrDefault(b => b.Id == item.Id);
				if (existingBatch == null) continue;

				if (pricing.Qty < existingBatch.Sold)
					return PurchaseErrors.CannotUpdateSoldQuantity(existingBatch.BatchName, pricing.Qty, existingBatch.Sold);

				var batchUpdateResult = existingBatch.Update(batchDetails);
				if (batchUpdateResult.IsError)
					return batchUpdateResult.Errors;
			}
			else
			{
				var batchResult = Batch.Create(batchDetails);
				if (batchResult.IsError)
					return batchResult.Errors;

				existingPurchase.AddBatch(batchResult.Value);
			}
		}

		await context.SaveChangesAsync(ct);
		await transaction.CommitAsync(ct);

		logger.LogInformation("Purchase {PurchaseId} updated with {BatchCount} batches", existingPurchase.Id, model.Items.Count);
		return existingPurchase.Id;
	}

	public async Task<ErrorOr<Deleted>> DeletePurchaseAsync(int id, CancellationToken ct = default)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Batches)
			.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (purchase == null)
			return PurchaseErrors.NotFound(id);

		if (purchase.Batches.Any(b => b.Sold > 0))
			return PurchaseErrors.CannotDeletePurchaseWithSoldBatches;

		context.Purchases.Remove(purchase);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Purchase {PurchaseId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<Purchase>> GetPurchaseForEditAsync(int id, CancellationToken ct = default)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Supplier)
			.Include(mp => mp.Batches)
				.ThenInclude(b => b.Medicine)
			.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (purchase == null)
			return PurchaseErrors.NotFound(id);

		return purchase;
	}

	public async Task<ErrorOr<Purchase>> GetPurchaseForViewAsync(int id, CancellationToken ct = default)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Supplier)
			.Include(mp => mp.Batches)
				.ThenInclude(b => b.Medicine)
			.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (purchase == null)
			return PurchaseErrors.NotFound(id);

		return purchase;
	}
}
