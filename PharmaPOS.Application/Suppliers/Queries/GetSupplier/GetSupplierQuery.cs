using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Suppliers.Queries.GetSupplier;

public record GetSupplierQuery(int Id) : IRequest<ErrorOr<SupplierResponse>>;
