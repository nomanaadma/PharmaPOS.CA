using MediatR;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Reports.Queries.GetIncomeExpenseReport;

public record GetIncomeExpenseReportQuery(InventoryReportRequest Request) : IRequest<StatsReportResponse>;
