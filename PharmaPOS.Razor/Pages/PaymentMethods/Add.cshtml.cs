using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.PaymentMethods;

[Authorize(Policy = Permissions.PaymentMethod.Add)]
public class AddModel(PaymentMethodService paymentMethodService) : BasePageModel
{
    [BindProperty]
    public PaymentMethodViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<PaymentMethodRequest>();

        var result = await paymentMethodService.CreatePaymentMethodAsync(request, ct);
        if (result.IsError)
            return HandleErrors(result.Errors, new() { ["PaymentMethod.NameNotUnique"] = "Input.Name" });

        NotifySuccess("Payment method created successfully.");
        return RedirectToPage("/PaymentMethods/Index");
    }
}
