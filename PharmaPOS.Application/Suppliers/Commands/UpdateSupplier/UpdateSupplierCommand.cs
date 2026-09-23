using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(int Id, SupplierRequest Request) : IRequest<ErrorOr<Updated>>;
