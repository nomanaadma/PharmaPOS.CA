using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Razor.Mappers;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Reports;

[Authorize(Policy = Permissions.Reports.View)]
public class InventoryModel(ReportService reportService) : BasePageModel
{
    public IEnumerable<InventoryReportResponse> Inventory { get; set; } = new List<InventoryReportResponse>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
	    Inventory = await reportService.GetInventoryReportAsync(ct);
        return Page();
    }
}
