using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(CustomerRequest Request) : IRequest<ErrorOr<Created>>;
