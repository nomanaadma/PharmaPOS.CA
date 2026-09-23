using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.GetFilteredBills;

public class GetFilteredBillsQueryHandler(
	IBillRepository billRepo) : IRequestHandler<GetFilteredBillsQuery, DataTableResponse<BillingListResponse>>
{
	public async Task<DataTableResponse<BillingListResponse>> Handle(GetFilteredBillsQuery query, CancellationToken ct)
	{
		return await billRepo.GetFilteredBillsAsync(query.Request, query.Columns, ct);
	}
}
