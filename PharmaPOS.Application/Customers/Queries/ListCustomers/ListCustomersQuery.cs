using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Customers.Queries.ListCustomers;

public record ListCustomersQuery : IRequest<List<CustomerResponse>>;
