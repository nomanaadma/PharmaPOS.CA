using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Medicines;

[Authorize(Policy = Permissions.Medicine.View)]
public class ViewModel(MedicineService medicineService) : BasePageModel
{
    public MedicineViewModel Medicine { get; set; } = new();
    public IEnumerable<StockViewModel> LiveStock { get; set; } = new List<StockViewModel>();
    public IEnumerable<StockViewModel> BadStock { get; set; } = new List<StockViewModel>();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await medicineService.GetMedicineAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        var medicineCategory = result.Value;
        var stockSummary = await medicineService.GetMedicineStockSummaryAsync(id, ct);
        
        Medicine = MedicineMapper.MapToMedicine(medicineCategory);
        LiveStock = StockMapper.MapToStocksBatch(stockSummary.ActiveBatches, stockSummary.MedicineNames);
        BadStock = StockMapper.MapToStocksBatch(stockSummary.ExpiredBatches, stockSummary.MedicineNames);

        return Page();
    }
}
