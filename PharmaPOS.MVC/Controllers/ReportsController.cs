using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class ReportsController(ReportService reportService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public IActionResult Index(string? name)
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> Inventory(CancellationToken ct = default)
    {
		var inventory = await reportService.GetInventoryReportAsync(ct);

		var inventoryDto = ReportMapper.MapToInventoryReport(inventory);

        return View(inventoryDto);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> IncomeExpense(DateTime? start, DateTime? end, int? medicineId, string? medicineName, CancellationToken ct = default)
    {
        var stats = await reportService.GetIncomeExpenseReportAsync(start, end, medicineId, medicineName, ct);

		var statsDto = ReportMapper.MapToStatsReport(stats);
        
        return View(statsDto);
    }
}
