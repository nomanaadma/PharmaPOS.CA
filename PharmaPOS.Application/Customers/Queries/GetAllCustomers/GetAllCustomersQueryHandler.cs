using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler(
	ICustomerRepository customerRepo) : IRequestHandler<GetAllCustomersQuery, List<CustomerResponse>>
{
	public async Task<List<CustomerResponse>> Handle(GetAllCustomersQuery query, CancellationToken ct)
	{
		var customers = await customerRepo.GetAllAsync(ct);
		var customerResponse = customers.Adapt<List<CustomerResponse>>();
		return customerResponse;
	}
}
