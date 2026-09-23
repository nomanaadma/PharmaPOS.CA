using MediatR;
using PharmaPOS.Application.Helpers;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Purchases.Queries.GetFilteredPurchases;

public record GetFilteredPurchasesQuery(
	DataTableRequest Request, 
	DataTableColumnMap<PurchaseListResponse> Columns) : IRequest<DataTableResponse<PurchaseListResponse>>;
