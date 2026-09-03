using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Mappers;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Stock;

[Authorize(Policy = Permissions.Stock.View)]
public class IndexModel(StockService stockService) : BasePageModel
{
    public IEnumerable<StockViewModel> Stocks { get; set; } = new List<StockViewModel>();

    public async Task<IActionResult> OnGetAsync(string? type, CancellationToken ct)
    {
        var result = await stockService.GetStockListAsync(type, ct);
        Stocks = StockMapper.MapToStockList(result);

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

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, int medicineId, decimal available, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Stock.View);
        if (!hasPermission)
            return Forbid();
            
        var refererUrl = Request.Headers.Referer.ToString();
        if (string.IsNullOrEmpty(refererUrl))
        {
            refererUrl = "/Medicine/Stock";
        }
        
        var result = await stockService.UpdateStockAvailableAsync(id, medicineId, available, ct);

        if (result.IsError)
        {
            NotifyWarning(result.Errors[0].Description);
            return Redirect(refererUrl);
        }

        NotifySuccess("Stock updated successfully!");
        return Redirect(refererUrl);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Stock.Delete);
        if (!hasPermission)
            return Forbid();
            
        var refererUrl = Request.Headers.Referer.ToString();
        if (string.IsNullOrEmpty(refererUrl))
        {
            refererUrl = "/Medicine/Stock";
        }

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
