using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.PaymentMethods;

[Authorize(Policy = Permissions.PaymentMethod.List)]
public class IndexModel(PaymentMethodService paymentMethodService) : BasePageModel
{
    public IEnumerable<PaymentMethodViewModel> PaymentMethods { get; set; } = new List<PaymentMethodViewModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var methodsQuery = await paymentMethodService.GetAllPaymentMethodsAsync(ct);
        PaymentMethods = SettingMapper.MapToPaymentMethods(methodsQuery);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.PaymentMethod.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await paymentMethodService.DeletePaymentMethodAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Payment method deleted successfully.");
        return RedirectToPage();
    }
}
