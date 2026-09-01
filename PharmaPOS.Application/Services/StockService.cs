using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Data;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Application.Services;

public class StockService(
	ApplicationDbContext context,
	MedicineService medicineService,
	BatchService batchService,
	ILogger<StockService> logger) : IApplicationService
{
	public async Task<List<StockResponse>> GetStockListAsync(string? type, CancellationToken ct = default)
	{
		var batchesQuery = batchService.ActiveBatchesQuery();

		var today = DateTime.UtcNow.Date;
		var currentMonthDate = new DateTime(today.Year, today.Month, 1);
		
		var currentMonthStr = currentMonthDate.ExpiryFormat();

		var pageType = type ?? "live";
		
		switch (pageType)
		{
			case "expired":
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) < 0);
				break;
				
			case "willexpirein1":
				var oneMonthStr = currentMonthDate.AddMonths(1).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), oneMonthStr) <= 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
				break;
				
			case "willexpirein2":
				var twoMonthStr = currentMonthDate.AddMonths(2).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), twoMonthStr) <= 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
				break;
				
			case "willexpirein3":
				var threeMonthStr = currentMonthDate.AddMonths(3).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), threeMonthStr) < 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) > 0);
				break;
				
			case "willexpirein6":
				var sixMonthStr = currentMonthDate.AddMonths(6).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), sixMonthStr) < 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) > 0);
				break;
				
			default:
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
				break;
		}
		
		var batches = await batchesQuery.ToListAsync(ct);
		var batchIds = batches.Select(b => b.Id).ToList();
		
		var medicineIds = batches.Select(b => b.MedicineId).Distinct().ToList();
		var medicineNames = await medicineService.GetMedicineNamesByIds(medicineIds, ct);
		
		var billItems = await context.BillItems
			.Where(bi => batchIds.Contains(bi.BatchId))
			.ToListAsync(ct);	
		
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

	public async Task<ErrorOr<Success>> UpdateStockAvailableAsync(int id, int medicineId, decimal available, CancellationToken ct = default)
	{
		var batch = await batchService.GetSingleBatchAsync(medicineId, id, ct);
		
		if (batch == null)
			return Error.NotFound(description: "Stock does not exist in database!");

		batch.UpdateSold(batch.Pricing.Qty - available);
		await context.SaveChangesAsync(ct);
		
		logger.LogInformation("Stock {BatchId} for Medicine {MedicineId} updated to available: {Available}", id, medicineId, available);
		return Result.Success;
	}

	public async Task<ErrorOr<Success>> DeleteStockAsync(int id, CancellationToken ct = default)
	{
		var batch = await batchService.GetSingleBatchByIdAsync(id, ct);
		
		if (batch == null)
			return Error.NotFound(description: "Stock does not exist in database!");
		
		batch.UpdateStatus(Status.InActive);
		await context.SaveChangesAsync(ct);
		
		logger.LogInformation("Stock {BatchId} marked as InActive", id);
		return Result.Success;
	}
}
