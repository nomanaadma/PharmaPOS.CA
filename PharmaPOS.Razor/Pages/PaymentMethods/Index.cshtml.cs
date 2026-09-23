using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.PaymentMethods.Commands.DeletePaymentMethod;
using PharmaPOS.Application.PaymentMethods.Queries.GetAllPaymentMethods;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Razor.Pages.PaymentMethods;

[Authorize(Policy = Permissions.PaymentMethod.List)]
public class IndexModel(ISender sender) : BasePageModel
{
    public IEnumerable<PaymentMethodResponse> PaymentMethods { get; set; } = new List<PaymentMethodResponse>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var methods = await sender.Send(new GetAllPaymentMethodsQuery(), ct);
        PaymentMethods = methods;
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.PaymentMethod.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await sender.Send(new DeletePaymentMethodCommand(id), ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Payment method deleted successfully.");
        return RedirectToPage();
    }
}
