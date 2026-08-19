using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.Domain.Enums;
using PharmaPOS.MVC.Models;
using System.Text.Json;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Mappers;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("Medicine/Stock/[action]")]
public class StockController(ApplicationDbContext context) : BaseController
{
	[HttpGet("/Medicine/Stock")]
	[Authorize(Policy = Permissions.Stock.View)]
	public async Task<IActionResult> Index(string? type, CancellationToken ct = default)
	{
		var batchesQuery = context.Batches
			.Include(b => b.Medicine)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId)
			.AsQueryable();

		var today = DateTime.UtcNow.Date;
		var currentMonthDate = new DateTime(today.Year, today.Month, 1);
		
		var currentMonthStr = currentMonthDate.ExpiryFormat();

		string pageTitle;
		var pageType = type ?? "live";
		
		switch (pageType)
		{
			case "expired":
				pageTitle = "Expired Stock";
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) < 0);
				break;
				
			case "willexpirein1":
				pageTitle = "Expire in 1 Month";
				var oneMonthStr = currentMonthDate.AddMonths(1).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), oneMonthStr) <= 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
				break;
				
			case "willexpirein2":
				pageTitle = "Expire in 2 Month";
				var twoMonthStr = currentMonthDate.AddMonths(2).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), twoMonthStr) <= 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
				break;
				
			case "willexpirein3":
				pageTitle = "Expire in 3 Month";
				var threeMonthStr = currentMonthDate.AddMonths(3).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), threeMonthStr) < 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) > 0);
				break;
				
			case "willexpirein6":
				pageTitle = "Expire in 6 Month";
				var sixMonthStr = currentMonthDate.AddMonths(6).ExpiryFormat();
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), sixMonthStr) < 0 && 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) > 0);
				break;
				
			default:
				pageTitle = "Live Stock";
				batchesQuery = batchesQuery.Where(b => 
					string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
				break;
		}
		
		var batches = await batchesQuery.ToListAsync(ct);
		
		var batchIds = batches.Select(b => b.Id).ToList();
		
		var billItems = await context.BillItems
			.Where(bi => batchIds.Contains(bi.BatchId))
			.ToListAsync(ct);
		
		var result = batches.Select(b =>
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
		
		ViewData["DeleteMsg"] = "Stock will be removed from Inventory.";
		ViewData["Title"] = pageTitle;
		
		return View(result);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Stock.View)]
	public async Task<IActionResult> Update(int id, int medicineId, decimal available, CancellationToken ct = default)
	{
		var batch = await context.Batches
			
			.FirstOrDefaultAsync(b => b.Id == id && b.MedicineId == medicineId, ct);

		var refererUrl = Request.Headers.Referer.ToString();

		if (batch == null)
		{
			NotifyWarning("Stock does not exist in database!");
			return Redirect(refererUrl);
		}

		batch.UpdateSold(batch.Pricing.Qty - available);
		await context.SaveChangesAsync(ct);

		NotifySuccess("Stock updated successfully!");
		return Redirect(refererUrl);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Stock.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var batch = await context.Batches
			
			.FirstOrDefaultAsync(b => b.Id == id, ct);
		
		var refererUrl = Request.Headers.Referer.ToString();
		
		if (batch == null)
		{
			NotifyWarning("Stock does not exist in database!");
			return Redirect(refererUrl);
		}
		
		batch.UpdateStatus(Status.InActive);
		await context.SaveChangesAsync(ct);
		
		NotifySuccess("Medicine Stock deleted successfully.");
		return Redirect(refererUrl);
		
	}
}
