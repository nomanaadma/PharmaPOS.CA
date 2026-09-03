using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Medicines;

[Authorize(Policy = Permissions.Medicine.List)]
public class IndexModel(MedicineService medicineService) : BasePageModel
{
    public IEnumerable<MedicineViewModel> Medicines { get; set; } = new List<MedicineViewModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var medicinesCategories = await medicineService.GetMedicinesWithStockAsync(ct);
        Medicines = MedicineMapper.MapToMedicinesWithStock(medicinesCategories);
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Medicine.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await medicineService.DeleteMedicineAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Medicine deleted successfully.");
        return RedirectToPage();
    }
}
