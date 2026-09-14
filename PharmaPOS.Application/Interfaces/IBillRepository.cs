using PharmaPOS.Application.Helpers;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BillingAggregate;

namespace PharmaPOS.Application.Interfaces;

public interface IBillRepository : IRepository<Bill>
{
	Task<DataTableResponse<Bill>> GetFilteredBillsAsync(
		DataTableRequest request, 
		DataTableColumnMap<Bill> columns, 
		CancellationToken ct = default);

	Task<Bill?> GetBillWithItemsAsync(int id, CancellationToken ct = default);
	Task<BillDetailsResponse?> GetBillDetailsAsync(int id, CancellationToken ct = default);
	Task<List<Bill>> GetRecentBillsAsync(int customerId, string? email, CancellationToken ct = default);
	Task<List<BillItem>> GetBillItemsForBatchesAsync(List<int> batchIds, CancellationToken ct = default);
}
