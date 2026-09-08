using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Extensions;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Purchase;

[Authorize(Policy = Permissions.Purchase.Add)]
public class AddModel(PurchaseService purchaseService, SupplierService supplierService) : BasePageModel
{
    [BindProperty]
    public PurchaseFormModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Input.Suppliers = (await supplierService.GetSuppliersAsync(ct)).ToSelectList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Input.Suppliers = (await supplierService.GetSuppliersAsync(ct)).ToSelectList();
        
        if (!ModelState.IsValid)
        {
            NotifyError("Please check your inputs.");
            return Page();
        }
        
        var request = Input.Adapt<PurchaseRequest>();
        
        var result = await purchaseService.CreatePurchaseAsync(request, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Purchase created successfully.");
        return RedirectToPage("/Purchase/Index");
    }
}
