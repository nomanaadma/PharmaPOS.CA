using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Razor.Pages.Reports;

[Authorize(Policy = Permissions.Reports.View)]
public class IncomeExpenseModel(ReportService reportService) : BasePageModel
{
	
	[BindProperty(SupportsGet = true)]
	public InventoryReportRequest Filter { get; set; } = new();

	public StatsReportResponse Stats { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(InventoryReportRequest request, CancellationToken ct)
    {
	    Stats = await reportService.GetIncomeExpenseReportAsync(request, ct);
        return Page();
    }
}
