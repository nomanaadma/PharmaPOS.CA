using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Razor.Pages.Customers;

[Authorize(Policy = Permissions.Medicine.List)]
public class GetCustomerModel(CustomerService customerService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string term, CancellationToken ct)
    {
        var canAdd = User.HasClaim("Permission", Permissions.Billing.Add);
        var canEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
        
        if (!canAdd && !canEdit)
        {
            return Forbid();
        }
        
        if (string.IsNullOrWhiteSpace(term))
        {
            return new JsonResult(Array.Empty<object>());
        }

        var results = await customerService.SearchCustomersAsync(term, ct);
        
        return new JsonResult(results);
    }
}
