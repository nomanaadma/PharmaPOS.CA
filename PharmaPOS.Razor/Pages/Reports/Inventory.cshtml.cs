using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Mappers;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Reports;

[Authorize(Policy = Permissions.Reports.View)]
public class InventoryModel(ReportService reportService) : BasePageModel
{
    public IEnumerable<InventoryReportViewModel> Inventory { get; set; } = new List<InventoryReportViewModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var inventory = await reportService.GetInventoryReportAsync(ct);
        Inventory = ReportMapper.MapToInventoryReport(inventory);
        return Page();
    }
}
