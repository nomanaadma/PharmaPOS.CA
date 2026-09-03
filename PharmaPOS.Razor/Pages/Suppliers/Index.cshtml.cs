using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Suppliers;

[Authorize(Policy = Permissions.Suppliers.List)]
public class IndexModel(SupplierService supplierService) : BasePageModel
{
    public IEnumerable<SupplierViewModel> Suppliers { get; set; } = new List<SupplierViewModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var suppliersQuery = await supplierService.GetAllSuppliersAsync(ct);
        Suppliers = SettingMapper.MapToSuppliers(suppliersQuery);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Suppliers.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await supplierService.DeleteSupplierAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Supplier deleted successfully.");
        return RedirectToPage();
    }
}
