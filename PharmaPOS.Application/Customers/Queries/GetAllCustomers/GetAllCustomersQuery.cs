using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery : IRequest<List<CustomerResponse>>;
