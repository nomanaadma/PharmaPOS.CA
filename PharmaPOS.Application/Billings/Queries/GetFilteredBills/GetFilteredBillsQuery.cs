using MediatR;
using PharmaPOS.Application.Helpers;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.GetFilteredBills;

public record GetFilteredBillsQuery(
	DataTableRequest Request, 
	DataTableColumnMap<BillingListResponse> Columns) : IRequest<DataTableResponse<BillingListResponse>>;
