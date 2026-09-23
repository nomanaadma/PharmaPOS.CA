using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(int Id, CustomerRequest Request) : IRequest<ErrorOr<Updated>>;
