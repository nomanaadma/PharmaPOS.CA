using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Data;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Application.Services;

public class BatchService(
	ApplicationDbContext context
	) : IApplicationService
{
	
	private static string CurrentMonth => DateTime.UtcNow.ExpiryFormat();
	
	
	public async Task<List<Batch>> GetNonExpiredBatchesAsync(List<int> medicineIds, CancellationToken ct)
	{
		
		
		// Get active, non-expired batches for the medicines
		var activeBatches = await context.Batches
			.Where(b => medicineIds.Contains(b.MedicineId))
			.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), CurrentMonth) >= 0)
			.Where(b => b.Status == Status.Active)
			.ToListAsync(ct);
		return activeBatches;
	}
	
	public async Task<List<Batch>> GetNonExpiredBatchesForMedicineAsync(int medicineId, CancellationToken ct = default)
	{

		return await context.Batches
			.Where(b => b.MedicineId == medicineId)
			.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), CurrentMonth) >= 0)
			.Where(b => (b.Pricing.Qty - b.Sold) > 0)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.Expiry)
			.ToListAsync(ct);
	}
	
	public async Task<List<Batch>> GetActiveBatchesByMedicineAsync(int medicineId, CancellationToken ct)
	{
		var batchesQuery = await context.Batches
			.Where(b => b.MedicineId == medicineId)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId).Include(batch => batch.Pricing)
			.ToListAsync(ct);
		return batchesQuery;
	}
	
	public async Task<Batch?> GetSingleBatchAsync(int medicineId, int batchId, CancellationToken ct = default)
	{
		return await context.Batches.Include(b => b.Pricing)
			.FirstOrDefaultAsync(b => b.MedicineId == medicineId && b.Id == batchId, ct);
	}
	
	public async Task<Dictionary<int, Batch>> GetBatchByIdsAsync(List<int> batchIds, CancellationToken ct)
	{
		var batchesById = await context.Batches
			.Where(b => batchIds.Contains(b.Id))
			.ToDictionaryAsync(b => b.Id, ct);

		return batchesById;
	}
	
	public async Task<List<Batch>> GetBatchesByPurchaseIdAsync(int purchaseId, CancellationToken ct)
	{
		var batches = await context.Batches
			.Where(b => b.PurchaseId == purchaseId)
			.ToListAsync(ct);

		return batches;
	}
	
	public async Task<bool> HasSoldBatchesAsync(int id, CancellationToken ct)
	{
		var hasSoldBatches = await context.Batches
			.AnyAsync(b => b.PurchaseId == id && b.Sold > 0, ct);
		return hasSoldBatches;
	}
	
	public async Task DeleteBatchesByPurchaseIdAsync(int purchaseId, CancellationToken ct)
	{
		await context.Batches
			.Where(b => b.PurchaseId == purchaseId)
			.ExecuteDeleteAsync(ct);
	}
	
	public IQueryable<Batch> ActiveBatchesQuery()
	{
		var batchesQuery = context.Batches
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId)
			.AsQueryable();
		return batchesQuery;
	}
	
	public async Task<Batch?> GetSingleBatchByIdAsync(int id, CancellationToken ct)
	{
		var batch = await context.Batches
			.FirstOrDefaultAsync(b => b.Id == id, ct);
		return batch;
	}
	
}