using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Extensions;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Billing;

[Authorize(Policy = Permissions.Billing.Edit)]
public class EditModel(BillingService billingService, PaymentMethodService paymentMethodService) : BasePageModel
{
    [BindProperty]
    public BillingFormModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await billingService.GetBillForEditAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        var paymentMethods = (await paymentMethodService.GetPaymentMethodsAsync(ct)).ToSelectList();
        
        Input = BillingMapper.MapToBillingForm(result.Value, paymentMethods);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (id != Input.Id)
        {
            NotifyError("Bad Request: ID mismatch.");
            return BadRequest();
        }
        
        Input.PaymentMethods = (await paymentMethodService.GetPaymentMethodsAsync(ct)).ToSelectList();

        if (!ModelState.IsValid)
        {
            NotifyError("Please check your inputs.");
            return Page();
        }
        
        var request = Input.Adapt<BillingRequest>();

        var result = await billingService.UpdateBillAsync(id, request, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Bill updated successfully.");
        return RedirectToPage("/Billing/View", new { id });
    }
}
