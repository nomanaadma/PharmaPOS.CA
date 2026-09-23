using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Suppliers.Queries.GetAllSuppliers;

public record GetAllSuppliersQuery : IRequest<List<SupplierResponse>>;
