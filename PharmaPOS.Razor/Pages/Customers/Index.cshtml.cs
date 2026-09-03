using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Customers;

[Authorize(Policy = Permissions.Customer.List)]
public class IndexModel(CustomerService customerService) : BasePageModel
{
    public IEnumerable<CustomerViewModel> Customers { get; set; } = new List<CustomerViewModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var customersQuery = await customerService.GetAllCustomersAsync(ct);
        Customers = CustomerMapper.MapToCustomers(customersQuery);
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Customer.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await customerService.DeleteCustomerAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Customer deleted successfully.");
        return RedirectToPage();
    }
}
