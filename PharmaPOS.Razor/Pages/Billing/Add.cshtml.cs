using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Billing;

[Authorize(Policy = Permissions.Billing.Add)]
public class AddModel(BillingService billingService, PaymentMethodService paymentMethodService) : BasePageModel
{
    [BindProperty]
    public BillingFormModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Input.PaymentMethods = await paymentMethodService.GetPaymentMethodsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Input.PaymentMethods = await paymentMethodService.GetPaymentMethodsAsync(ct);
        
        if (!ModelState.IsValid)
        {
            NotifyError("Please check your inputs.");
            return Page();
        }
        
        var request = Input.Adapt<BillingRequest>();

        var result = await billingService.CreateBillAsync(request, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Bill created successfully.");
        return RedirectToPage("/Billing/View", new { id = result.Value });
    }
}
