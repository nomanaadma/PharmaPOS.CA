using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(int Id) : IRequest<ErrorOr<Deleted>>;
