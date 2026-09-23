using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(SupplierRequest Request) : IRequest<ErrorOr<Created>>;
