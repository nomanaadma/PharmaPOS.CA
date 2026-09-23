using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Suppliers.Commands.DeleteSupplier;
using PharmaPOS.Application.Suppliers.Queries.GetAllSuppliers;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Suppliers;

[Authorize(Policy = Permissions.Suppliers.List)]
public class IndexModel(ISender sender) : BasePageModel
{
    public IEnumerable<SupplierResponse> Suppliers { get; set; } = new List<SupplierResponse>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Suppliers = await sender.Send(new GetAllSuppliersQuery(), ct);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Suppliers.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await sender.Send(new DeleteSupplierCommand(id), ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Supplier deleted successfully.");
        return RedirectToPage();
    }
}
