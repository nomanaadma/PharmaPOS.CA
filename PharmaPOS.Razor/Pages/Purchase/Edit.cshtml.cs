using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Extensions;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Purchase;

[Authorize(Policy = Permissions.Purchase.Edit)]
public class EditModel(PurchaseService purchaseService, SupplierService supplierService) : BasePageModel
{
    [BindProperty]
    public PurchaseFormModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await purchaseService.GetPurchaseAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);
        
        var suppliers = (await supplierService.GetSuppliersAsync(ct)).ToSelectList();
        
        Input = PurchaseMapper.MapToPurchaseForm(result.Value, suppliers);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (id != Input.Id)
        {
            NotifyError("Bad Request: ID mismatch.");
            return BadRequest();
        }
        
        Input.Suppliers = (await supplierService.GetSuppliersAsync(ct)).ToSelectList();

        if (!ModelState.IsValid)
        {
            NotifyError("Please check your inputs.");
            return Page();
        }
        
        var request = Input.Adapt<PurchaseRequest>();

        var result = await purchaseService.UpdatePurchaseAsync(id, request, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Purchase updated successfully.");
        return RedirectToPage("/Purchase/Index");
    }
}
