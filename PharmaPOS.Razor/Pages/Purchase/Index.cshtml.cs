using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Razor.Pages.Purchase;

[Authorize(Policy = Permissions.Purchase.List)]
public class IndexModel(PurchaseService purchaseService) : BasePageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Purchase.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await purchaseService.DeletePurchaseAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Purchase deleted successfully.");
        return RedirectToPage();
    }
}
