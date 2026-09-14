using PharmaPOS.Application.Helpers;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.PurchaseAggregate;

namespace PharmaPOS.Application.Interfaces;

public interface IPurchaseRepository : IRepository<Purchase>
{
	Task<DataTableResponse<PurchaseListResponse>> GetFilteredPurchasesAsync(
		DataTableRequest request, 
		DataTableColumnMap<PurchaseListResponse> columns, 
		CancellationToken ct = default);

	Task<PurchaseDetailsResponse?> GetPurchaseDetailsAsync(int id, CancellationToken ct = default);
}
