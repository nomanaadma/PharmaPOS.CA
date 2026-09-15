using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Extensions;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Medicines;

[Authorize(Policy = Permissions.Medicine.Edit)]
public class EditModel(MedicineService medicineService, CategoryService categoryService) : BasePageModel
{
    [BindProperty]
    public MedicineViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await medicineService.GetMedicineAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);
        
        var categories = (await categoryService.GetCategoriesAsync(ct)).ToSelectList();
        Input = MedicineMapper.MapToMedicine(result.Value, categories);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        Input.Categories = (await categoryService.GetCategoriesAsync(ct)).ToSelectList();
            
        if (id != Input.Id)
        {
            NotifyError("Bad Request: ID mismatch.");
            return BadRequest();
        }
        
        if (Input.Id == 0)
        {
            NotifyError("Medicine ID is required for editing.");
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<MedicineRequest>();

        var result = await medicineService.UpdateMedicineAsync(id, request, ct);

        if (result.IsError)
        {
            return HandleErrors(result.Errors);
        }

        NotifySuccess("Medicine updated successfully.");
        return RedirectToPage("/Medicines/Index");
    }
}
