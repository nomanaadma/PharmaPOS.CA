using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.PaymentMethods;

[Authorize(Policy = Permissions.PaymentMethod.Edit)]
public class EditModel(PaymentMethodService paymentMethodService) : BasePageModel
{
    [BindProperty]
    public PaymentMethodViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await paymentMethodService.GetPaymentMethodAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        Input = SettingMapper.MapToPaymentMethod(result.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (id != Input.Id)
        {
            NotifyError("Bad Request: ID mismatch.");
            return BadRequest();
        }
        
        if (Input.Id == 0)
        {
            NotifyError("Payment Method ID is required for editing.");
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<PaymentMethodRequest>();

        var result = await paymentMethodService.UpdatePaymentMethodAsync(id, request, ct);
        if (result.IsError)
            return HandleErrors(result.Errors, new() { ["PaymentMethod.NameNotUnique"] = "Input.Name" });

        NotifySuccess("Payment method updated successfully.");
        return RedirectToPage("/PaymentMethods/Index");
    }
}
