using ErrorOr;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Application.Services;

public class StockService(
	IBatchRepository batchRepo,
	IBillRepository billRepo,
	IUnitOfWork unitOfWork,
	MedicineService medicineService,
	ILogger<StockService> logger) : IApplicationService
{
	public async Task<List<StockResponse>> GetStockListAsync(string? type, CancellationToken ct = default)
	{
		var batches = await batchRepo.GetActiveBatchesAsync(type, ct);
		var batchIds = batches.Select(b => b.Id).ToList();
		
		var medicineIds = batches.Select(b => b.MedicineId).Distinct().ToList();
		var medicineNames = await medicineService.GetMedicineNamesByIds(medicineIds, ct);
		
		var billItems = await billRepo.GetBillItemsForBatchesAsync(batchIds, ct);	
		
		return batches.Select(b =>
		{
			var unitSalePrice = 0m;
			var totalSales = 0m;
			var profit = 0m;
			
			var unitPurchasePrice = Math.Round(b.Pricing.Price / b.Pricing.Qty, 2);
			var totalPurchase = Math.Round(unitPurchasePrice * b.Sold, 2);

			var batchBillItems = billItems.Where(bi => bi.BatchId == b.Id).Where(bi => bi.MedicineId == b.MedicineId).ToList();

			var medicineSalePrice = 0m;
			var medicineSaleQty = 0m;
			
			foreach (var billItem in batchBillItems)
			{
				medicineSalePrice += billItem.Pricing.Price;
				medicineSaleQty += billItem.Pricing.Qty;
			}

			if (medicineSalePrice != 0 && medicineSaleQty != 0)
			{
				unitSalePrice += Math.Round(medicineSalePrice / medicineSaleQty, 2);
				totalSales += Math.Round(unitSalePrice * b.Sold, 2);
				profit += Math.Round(totalSales - totalPurchase, 2);
			}
			
			return new StockResponse(
				Id: b.Id,
				MedicineId: b.MedicineId,
				PurchaseId: b.PurchaseId,
				Name: medicineNames.GetValueOrDefault(b.MedicineId, ""),
				Batch: b.BatchName,
				Expiry: b.Expiry,
				PurchasePrice: unitPurchasePrice,
				SalePrice: unitSalePrice,
				TotalPurchase: totalPurchase,
				TotalSales: totalSales,
				Profit: profit,
				Qty: b.Pricing.Qty,
				Sold: b.Sold,
				Available: b.Pricing.Qty - b.Sold
			);
			
		}).ToList();
	}

	public async Task<ErrorOr<Updated>> UpdateStockAvailableAsync(int id, int medicineId, decimal available, CancellationToken ct = default)
	{
		var batch = await batchRepo.GetSingleBatchAsync(medicineId, id, ct);
		
		if (batch == null)
			return Error.NotFound(description: "Stock does not exist in database!");
		
		var targetTotalSold = batch.Pricing.Qty - available;
		var quantityJustSold = targetTotalSold - batch.Sold;

		var soldResult = batch.UpdateSold(quantityJustSold);
		if (soldResult.IsError)
			return soldResult.Errors;
		
		await unitOfWork.SaveChangesAsync(ct);
		
		logger.LogInformation("Stock {BatchId} for Medicine {MedicineId} updated to available: {Available}", id, medicineId, available);
		return Result.Updated;
	}

	public async Task<ErrorOr<Deleted>> DeleteStockAsync(int id, CancellationToken ct = default)
	{
		var batch = await batchRepo.GetByIdAsync(id, ct);
		
		if (batch == null)
			return Error.NotFound(description: "Stock does not exist in database!");
		
		batch.UpdateStatus(Status.InActive);
		await unitOfWork.SaveChangesAsync(ct);
		
		logger.LogInformation("Stock {BatchId} marked as InActive", id);
		return Result.Deleted;
	}
}
