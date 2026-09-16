using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Services;

public class ReportService(IReportRepository reportRepo) : IApplicationService
{
	public async Task<List<InventoryReportResponse>> GetInventoryReportAsync(CancellationToken ct = default)
	{
		return await reportRepo.GetInventoryReportAsync(ct);
	}

	public async Task<StatsReportResponse> GetIncomeExpenseReportAsync(InventoryReportRequest request, CancellationToken ct = default)
	{
		return await reportRepo.GetIncomeExpenseReportAsync(request, ct);
	}
}
