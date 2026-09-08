using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Extensions;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Medicines;

[Authorize(Policy = Permissions.Medicine.Add)]
public class AddModel(MedicineService medicineService, CategoryService categoryService) : BasePageModel
{
    [BindProperty]
    public MedicineViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Input.Categories = (await categoryService.GetCategoriesAsync(ct)).ToSelectList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Input.Categories = (await categoryService.GetCategoriesAsync(ct)).ToSelectList();
        
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<MedicineRequest>();

        var result = await medicineService.CreateMedicineAsync(request, ct);

        if (result.IsError)
        {
            return HandleErrors(result.Errors);
        }

        NotifySuccess("Medicine created successfully.");
        return RedirectToPage("/Medicines/Index");
    }
}
