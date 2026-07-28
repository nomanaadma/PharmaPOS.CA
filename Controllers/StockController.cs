using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Models;
using System.Globalization;
using System.Text.Json;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Medicine/Stock/[action]")]
public class StockController(ApplicationDbContext context) : Controller
{
	[HttpGet]
	[Route("/Medicine/Stock")]
	public async Task<IActionResult> Index([FromQuery] string? type)
	{
		var batchesRaw = await context.Batches
			.Include(b => b.Medicine)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId)
			.ToListAsync();

		var today = DateTime.UtcNow.Date;
		var currentMonthDate = new DateTime(today.Year, today.Month, 1);

		string pageTitle;
		string pageType = type ?? "live";

		// Parse expiry strings into real DateTimes for accurate logic.
		// Expiry is in "MM-yyyy" format.
		var batchesWithDates = batchesRaw.Select(b =>
		{
			DateTime.TryParseExact(b.Expiry, "MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiryDate);
			return new { Batch = b, ExpiryDate = expiryDate };
		}).ToList();

		var filteredBatches = batchesWithDates.AsEnumerable();

		switch (pageType)
		{
			case "expired":
				pageTitle = "Expired Stock";
				filteredBatches = batchesWithDates.Where(b => b.ExpiryDate < currentMonthDate);
				break;
			case "willexpirein1":
				pageTitle = "Expire in 1 Month";
				filteredBatches = batchesWithDates.Where(b => b.ExpiryDate > currentMonthDate && b.ExpiryDate < currentMonthDate.AddMonths(1));
				break;
			case "willexpirein2":
				pageTitle = "Expire in 2 Month";
				filteredBatches = batchesWithDates.Where(b => b.ExpiryDate > currentMonthDate && b.ExpiryDate < currentMonthDate.AddMonths(2));
				break;
			case "willexpirein3":
				pageTitle = "Expire in 3 Month";
				filteredBatches = batchesWithDates.Where(b => b.ExpiryDate > currentMonthDate && b.ExpiryDate < currentMonthDate.AddMonths(3));
				break;
			case "willexpirein6":
				pageTitle = "Expire in 6 Month";
				filteredBatches = batchesWithDates.Where(b => b.ExpiryDate > currentMonthDate && b.ExpiryDate < currentMonthDate.AddMonths(6));
				break;
			case "live":
			default:
				pageType = "live";
				pageTitle = "Live Stock";
				filteredBatches = batchesWithDates.Where(b => b.ExpiryDate > currentMonthDate);
				break;
		}

		var result = filteredBatches.Select(b => new StockViewModel
		{
			Id = b.Batch.Id,
			MedicineId = b.Batch.MedicineId,
			PurchaseId = b.Batch.PurchaseId,
			Name = b.Batch.Medicine.Name,
			Batch = b.Batch.BatchName,
			Expiry = b.Batch.Expiry,
			PurchasePrice = b.Batch.PurchasePrice,
			SalePrice = b.Batch.SalePrice,
			TotalPurchase = Math.Round(b.Batch.PurchasePrice * b.Batch.Qty, 2),
			TotalSales = Math.Round(b.Batch.SalePrice * b.Batch.Sold, 2),
			Profit = Math.Round((b.Batch.SalePrice * b.Batch.Sold) - (b.Batch.PurchasePrice * b.Batch.Sold), 2),
			Qty = b.Batch.Qty,
			Sold = b.Batch.Sold,
			Available = b.Batch.Qty - b.Batch.Sold
		}).ToList();

		ViewData["page_title"] = pageTitle;
		ViewData["type"] = pageType;

		return View(result);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("/Medicine/Stock/Update")]
	public async Task<IActionResult> Update(int id, int medicine_id, decimal available)
	{
		var batch = await context.Batches
			.AsTracking()
			.FirstOrDefaultAsync(b => b.Id == id && b.MedicineId == medicine_id);

		if (batch == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Stock does not exist in database!" });
			return RedirectToAction(nameof(Index));
		}

		batch.UpdateSold(batch.Qty - available);
		await context.SaveChangesAsync();

		TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Stock updated successfully!" });
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("/Medicine/Stock/Delete")]
	public async Task<IActionResult> Delete(int id)
	{
		var batch = await context.Batches
			.AsTracking()
			.FirstOrDefaultAsync(b => b.Id == id);

		if (batch != null)
		{
			batch.UpdateStatus(Status.InActive);
			await context.SaveChangesAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Medicine Stock deleted successfully." });
		}
		
		return RedirectToAction(nameof(Index));
	}
}
