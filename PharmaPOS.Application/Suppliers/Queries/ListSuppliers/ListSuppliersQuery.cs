using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Suppliers.Queries.ListSuppliers;

public record ListSuppliersQuery : IRequest<List<SupplierResponse>>;
