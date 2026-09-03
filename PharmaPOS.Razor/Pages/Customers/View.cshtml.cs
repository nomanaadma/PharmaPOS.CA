using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Customers;

[Authorize(Policy = Permissions.Customer.View)]
public class ViewModel(CustomerService customerService, BillingService billingService) : BasePageModel
{
    public CustomerViewModel Customer { get; set; } = new();
    public IEnumerable<CustomerBillViewModel> RecentBills { get; set; } = new List<CustomerBillViewModel>();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await customerService.GetCustomerAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        var customer = result.Value;
        var recentBills = await billingService.GetRecentBillsAsync(customer.Id, customer.Email, ct);
        
        Customer = CustomerMapper.MapToCustomer(customer);
        RecentBills = CustomerMapper.MapToCustomerBills(recentBills);

        return Page();
    }
}
