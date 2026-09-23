using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Suppliers.Queries.GetSupplierLookup;

public class GetSupplierLookupQueryHandler(
	ISupplierRepository supplierRepo) : IRequestHandler<GetSupplierLookupQuery, IEnumerable<LookupItem>>
{
	public async Task<IEnumerable<LookupItem>> Handle(GetSupplierLookupQuery query, CancellationToken ct)
	{
		return await supplierRepo.GetLookupListAsync(ct);
	}
}
