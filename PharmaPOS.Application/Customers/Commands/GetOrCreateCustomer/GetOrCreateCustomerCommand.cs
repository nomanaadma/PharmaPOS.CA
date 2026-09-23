using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Customers.Commands.GetOrCreateCustomer;

public record GetOrCreateCustomerCommand(CustomerRequest Request) : IRequest<ErrorOr<int>>;
