using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Razor.Pages.Purchase;

[Authorize(Policy = Permissions.Purchase.View)]
public class ViewModel(PurchaseService purchaseService) : BasePageModel
{
	public PurchaseResponse Purchase { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await purchaseService.GetPurchaseAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        Purchase = result.Value;
        
        return Page();
    }
}
