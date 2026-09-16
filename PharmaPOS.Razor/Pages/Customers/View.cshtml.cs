using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Razor.Pages.Customers;

[Authorize(Policy = Permissions.Customer.View)]
public class ViewModel(CustomerService customerService, BillingService billingService) : BasePageModel
{
	public CustomerResponse Customer { get; set; } = null!;
    public IEnumerable<CustomerBillsResponse> RecentBills { get; set; } = new List<CustomerBillsResponse>();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await customerService.GetCustomerAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        var customer = result.Value;
        var recentBills = await billingService.GetRecentBillsAsync(customer.Id, customer.Email, ct);

        Customer = customer;
        RecentBills = recentBills;

        return Page();
    }
}
