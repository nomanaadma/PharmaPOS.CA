using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Reports.Queries.GetInventoryReport;

public class GetInventoryReportQueryHandler(
	IReportRepository reportRepo) : IRequestHandler<GetInventoryReportQuery, List<InventoryReportResponse>>
{
	public async Task<List<InventoryReportResponse>> Handle(GetInventoryReportQuery query, CancellationToken ct)
	{
		return await reportRepo.GetInventoryReportAsync(ct);
	}
}
