using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int Id) : IRequest<ErrorOr<Deleted>>;
