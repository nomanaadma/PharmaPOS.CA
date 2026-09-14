using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Services;

public class ReportService(IReportRepository reportRepo) : IApplicationService
{
	public async Task<List<InventoryReportResponse>> GetInventoryReportAsync(CancellationToken ct = default)
	{
		return await reportRepo.GetInventoryReportAsync(ct);
	}

	public async Task<StatsReportResponse> GetIncomeExpenseReportAsync(
		DateTime? start, 
		DateTime? end, 
		int? medicineId, 
		string? medicineName, 
		CancellationToken ct = default)
	{
		return await reportRepo.GetIncomeExpenseReportAsync(start, end, medicineId, medicineName, ct);
	}
}
