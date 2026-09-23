using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Suppliers.Queries.GetAllSuppliers;

public class GetAllSuppliersQueryHandler(
	ISupplierRepository supplierRepo) : IRequestHandler<GetAllSuppliersQuery, List<SupplierResponse>>
{
	public async Task<List<SupplierResponse>> Handle(GetAllSuppliersQuery query, CancellationToken ct)
	{
		var suppliers = await supplierRepo.GetAllAsync(ct);
		var suppliersResponse = suppliers.Adapt<List<SupplierResponse>>();
		return suppliersResponse;
	}
}
