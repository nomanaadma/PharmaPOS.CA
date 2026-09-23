using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.GetRecentBills;

public class GetRecentBillsQueryHandler(
	IBillRepository billRepo) : IRequestHandler<GetRecentBillsQuery, List<CustomerBillsResponse>>
{
	public async Task<List<CustomerBillsResponse>> Handle(GetRecentBillsQuery query, CancellationToken ct)
	{
		var bills = await billRepo.GetRecentBillsAsync(query.CustomerId, query.Email, ct);
		var billsResponse = bills.Adapt<List<CustomerBillsResponse>>();
		return billsResponse;
	}
}
