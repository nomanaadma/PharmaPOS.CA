using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Reports.Queries.ListInventoryReport;

public record ListInventoryReportQuery : IRequest<List<InventoryReportResponse>>;
