using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("Medicine/Stock/[action]")]
public class StockController(StockService stockService) : BaseController
{
	[HttpGet("/Medicine/Stock")]
	[Authorize(Policy = Permissions.Stock.View)]
	public async Task<IActionResult> Index(string? type, CancellationToken ct = default)
	{
		var result = await stockService.GetStockListAsync(type, ct);

		var pageTitle = type switch
		{
			"expired" => "Expired Stock",
			"willexpirein1" => "Expire in 1 Month",
			"willexpirein2" => "Expire in 2 Month",
			"willexpirein3" => "Expire in 3 Month",
			"willexpirein6" => "Expire in 6 Month",
			_ => "Live Stock"
		};
		
		ViewData["DeleteMsg"] = "Stock will be removed from Inventory.";
		ViewData["Title"] = pageTitle;
		
		return View(result);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Stock.View)]
	public async Task<IActionResult> Update(int id, int medicineId, decimal available, CancellationToken ct = default)
	{
		var refererUrl = Request.Headers.Referer.ToString();
		
		var result = await stockService.UpdateStockAvailableAsync(id, medicineId, available, ct);

		if (result.IsError)
		{
			NotifyWarning(result.Errors[0].Description);
			return Redirect(refererUrl);
		}

		NotifySuccess("Stock updated successfully!");
		return Redirect(refererUrl);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Stock.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var refererUrl = Request.Headers.Referer.ToString();

		var result = await stockService.DeleteStockAsync(id, ct);
		
		if (result.IsError)
		{
			NotifyWarning(result.Errors[0].Description);
			return Redirect(refererUrl);
		}
		
		NotifySuccess("Medicine Stock deleted successfully.");
		return Redirect(refererUrl);
		
	}
}
