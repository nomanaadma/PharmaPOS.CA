using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Interfaces;

public interface IReportRepository
{
	Task<List<InventoryReportResponse>> GetInventoryReportAsync(CancellationToken ct = default);
	Task<StatsReportResponse> GetIncomeExpenseReportAsync(
		DateTime? start, 
		DateTime? end, 
		int? medicineId, 
		string? medicineName, 
		CancellationToken ct = default);
}
