using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Helpers;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Infrastructure.Common.Persistence;

namespace PharmaPOS.Infrastructure.Bills.Persistence;

public class BillRepository(
	ApplicationDbContext context, 
	IBatchRepository batchRepo,
	IPaymentMethodRepository paymentMethodRepository,
	IMedicineRepository medicineRepo) : Repository<Bill>(context), IBillRepository
{
	public async Task<DataTableResponse<Bill>> GetFilteredBillsAsync(
		DataTableRequest request, 
		DataTableColumnMap<Bill> columns, 
		CancellationToken ct = default)
	{
		var query = DbSet.AsQueryable();
		return await query.ToDataTableAsync(request, columns, ct);
	}

	public async Task<Bill?> GetBillWithItemsAsync(int id, CancellationToken ct = default)
	{
		return await DbSet
			.Include(b => b.Items)
			.FirstOrDefaultAsync(b => b.Id == id, ct);
	}

	public async Task<BillDetailsResponse?> GetBillDetailsAsync(int id, CancellationToken ct = default)
	{
		var bill = await DbSet
			.Include(b => b.Items)
			.FirstOrDefaultAsync(b => b.Id == id, ct);

		if (bill == null) return null;

		var batchIds = bill.Items.Select(i => i.BatchId).Distinct().ToList();
		var batchesById = await batchRepo.GetBatchByIdsAsync(batchIds, ct); 

		var medicineIds = bill.Items.Select(b => b.MedicineId).Distinct().ToList();
		var medicineNames = await medicineRepo.GetMedicineNamesByIdsAsync(medicineIds, ct);

		var paymentMethodName = await paymentMethodRepository.GetMethodNameByIdAsync(bill.MethodId, ct); 
		
		return new BillDetailsResponse(bill, batchesById, medicineNames, paymentMethodName);
	}

	public async Task<List<Bill>> GetRecentBillsAsync(int customerId, string? email, CancellationToken ct = default)
	{
		return await DbSet
			.Where(b => b.CustomerId == customerId || (email != null && b.Email == email))
			.OrderByDescending(b => b.Date)
			.Take(20)
			.ToListAsync(ct);
	}

	public async Task<List<BillItem>> GetBillItemsForBatchesAsync(List<int> batchIds, CancellationToken ct = default)
	{
		return await DbContext.BillItems
			.Where(bi => batchIds.Contains(bi.BatchId))
			.ToListAsync(ct);
	}
}
