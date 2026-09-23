using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Customers.Queries.SearchCustomers;

public record SearchCustomersQuery(string Term) : IRequest<List<CustomerSearchResult>>;
