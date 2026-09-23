using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Reports.Queries.GetInventoryReport;

public record GetInventoryReportQuery : IRequest<List<InventoryReportResponse>>;
