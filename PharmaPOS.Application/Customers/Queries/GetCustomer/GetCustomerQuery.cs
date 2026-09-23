using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Customers.Queries.GetCustomer;

public record GetCustomerQuery(int Id) : IRequest<ErrorOr<CustomerResponse>>;
