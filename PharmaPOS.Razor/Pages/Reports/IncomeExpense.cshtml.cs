using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Mappers;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Reports;

[Authorize(Policy = Permissions.Reports.View)]
public class IncomeExpenseModel(ReportService reportService) : BasePageModel
{
    public IncomeExpenseReportViewModel Stats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(DateTime? start, DateTime? end, int? medicineId, string? medicineName, CancellationToken ct)
    {
        var stats = await reportService.GetIncomeExpenseReportAsync(start, end, medicineId, medicineName, ct);
        Stats = ReportMapper.MapToStatsReport(stats);
        return Page();
    }
}
