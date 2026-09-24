using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Purchases.Queries.ListFilteredPurchases;

public class ListFilteredPurchasesQueryHandler(
	IPurchaseRepository purchaseRepo) : IRequestHandler<ListFilteredPurchasesQuery, DataTableResponse<PurchaseListResponse>>
{
	public async Task<DataTableResponse<PurchaseListResponse>> Handle(ListFilteredPurchasesQuery query, CancellationToken ct)
	{
		return await purchaseRepo.GetFilteredPurchasesAsync(query.Request, query.Columns, ct);
	}
}
