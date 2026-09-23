using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Suppliers.Queries.GetSupplierLookup;

public record GetSupplierLookupQuery : IRequest<IEnumerable<LookupItem>>;
