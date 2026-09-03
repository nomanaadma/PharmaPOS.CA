using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Suppliers;

[Authorize(Policy = Permissions.Suppliers.Add)]
public class AddModel(SupplierService supplierService) : BasePageModel
{
    [BindProperty]
    public SupplierViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<SupplierRequest>();

        var result = await supplierService.CreateSupplierAsync(request, ct);
        if (result.IsError)
            return HandleErrors(result.Errors, new() { ["Supplier.EmailNotUnique"] = "Input.Email" });

        NotifySuccess("Supplier created successfully.");
        return RedirectToPage("/Suppliers/Index");
    }
}
