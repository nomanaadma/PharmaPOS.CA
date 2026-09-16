using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Medicines;

[Authorize(Policy = Permissions.Medicine.View)]
public class ViewModel(MedicineService medicineService) : BasePageModel
{
    public MedicineResponse Medicine { get; set; } = null!;
    public IEnumerable<BatchResponse> LiveStock { get; set; } = new List<BatchResponse>();
    public IEnumerable<BatchResponse> BadStock { get; set; } = new List<BatchResponse>();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await medicineService.GetMedicineAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);
        
        var stockSummary = await medicineService.GetMedicineStockSummaryAsync(id, ct);

        Medicine = result.Value;
        LiveStock = stockSummary.liveBatches;
        BadStock = stockSummary.badBatches;
        
        return Page();
    }
}
