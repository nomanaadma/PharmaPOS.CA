using ErrorOr;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Data;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.MVC.DTOs;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class StockService(ApplicationDbContext context, ILogger<StockService> logger)
{
	public async Task<List<StockViewModel>> GetStockListAsync(string? type, CancellationToken ct = default)
	{
		var batchesQuery = context.Batches
			.Include(b => b.Medicine)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId)
			.AsQueryable();

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

			var batchBillItesm = billItems.Where(bi => bi.BatchId == b.Id).Where(bi => bi.MedicineId == b.MedicineId).ToList();

			var medicineSalePrice = 0m;
			var medicineSaleQty = 0m;
			
			foreach (var billItem in batchBillItesm)
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

			return StockMapper.MapToStock(b, false, unitPurchasePrice, unitSalePrice, totalPurchase, totalSales, profit);
			
		}).ToList();
	}

	public async Task<ErrorOr<Success>> UpdateStockAvailableAsync(int id, int medicineId, decimal available, CancellationToken ct = default)
	{
		var batch = await context.Batches
			.FirstOrDefaultAsync(b => b.Id == id && b.MedicineId == medicineId, ct);

		if (batch == null)
			return Error.NotFound(description: "Stock does not exist in database!");

		batch.UpdateSold(batch.Pricing.Qty - available);
		await context.SaveChangesAsync(ct);
		
		logger.LogInformation("Stock {BatchId} for Medicine {MedicineId} updated to available: {Available}", id, medicineId, available);
		return Result.Success;
	}

	public async Task<ErrorOr<Success>> DeleteStockAsync(int id, CancellationToken ct = default)
	{
		var batch = await context.Batches
			.FirstOrDefaultAsync(b => b.Id == id, ct);
		
		if (batch == null)
			return Error.NotFound(description: "Stock does not exist in database!");
		
		batch.UpdateStatus(Status.InActive);
		await context.SaveChangesAsync(ct);
		
		logger.LogInformation("Stock {BatchId} marked as InActive", id);
		return Result.Success;
	}
}
