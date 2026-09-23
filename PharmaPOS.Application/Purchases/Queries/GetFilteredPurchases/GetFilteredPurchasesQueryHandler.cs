using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Purchases.Queries.GetFilteredPurchases;

public class GetFilteredPurchasesQueryHandler(
	IPurchaseRepository purchaseRepo) : IRequestHandler<GetFilteredPurchasesQuery, DataTableResponse<PurchaseListResponse>>
{
	public async Task<DataTableResponse<PurchaseListResponse>> Handle(GetFilteredPurchasesQuery query, CancellationToken ct)
	{
		return await purchaseRepo.GetFilteredPurchasesAsync(query.Request, query.Columns, ct);
	}
}
