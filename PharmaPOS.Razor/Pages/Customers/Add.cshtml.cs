using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Customers;

[Authorize(Policy = Permissions.Customer.Add)]
public class AddModel(CustomerService customerService) : BasePageModel
{
    [BindProperty]
    public CustomerViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<CustomerRequest>();

        var result = await customerService.CreateCustomerAsync(request, ct);
        if (result.IsError)
            return HandleErrors(result.Errors, new() { ["Customer.EmailNotUnique"] = "Input.Email" });

        NotifySuccess("Customer created successfully.");
        return RedirectToPage("/Customers/Index");
    }
}
