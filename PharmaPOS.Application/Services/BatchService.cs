using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Application.Services;

public class BatchService(IRepository<Batch> batchRepo) : IApplicationService
{
	private static string CurrentMonth => DateTime.UtcNow.ExpiryFormat();

	public async Task<List<Batch>> GetNonExpiredBatchesAsync(List<int> medicineIds, CancellationToken ct)
	{
		// Get active, non-expired batches for the medicines
		return await batchRepo.Query()
			.Where(b => medicineIds.Contains(b.MedicineId))
			.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), CurrentMonth) >= 0)
			.Where(b => b.Status == Status.Active)
			.ToListAsync(ct);
	}

	public async Task<List<Batch>> GetNonExpiredBatchesForMedicineAsync(int medicineId, CancellationToken ct = default)
	{
		return await batchRepo.Query()
			.Where(b => b.MedicineId == medicineId)
			.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), CurrentMonth) >= 0)
			.Where(b => (b.Pricing.Qty - b.Sold) > 0)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.Expiry)
			.ToListAsync(ct);
	}

	public async Task<List<Batch>> GetActiveBatchesByMedicineAsync(int medicineId, CancellationToken ct)
	{
		return await batchRepo.Query()
			.Where(b => b.MedicineId == medicineId)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId)
			.Include(batch => batch.Pricing)
			.ToListAsync(ct);
	}

	public async Task<Batch?> GetSingleBatchAsync(int medicineId, int batchId, CancellationToken ct = default)
	{
		return await batchRepo.Query()
			.Include(b => b.Pricing)
			.FirstOrDefaultAsync(b => b.MedicineId == medicineId && b.Id == batchId, ct);
	}

	public async Task<Dictionary<int, Batch>> GetBatchByIdsAsync(List<int> batchIds, CancellationToken ct)
	{
		return await batchRepo.Query()
			.Where(b => batchIds.Contains(b.Id))
			.ToDictionaryAsync(b => b.Id, ct);
	}

	public async Task<List<Batch>> GetBatchesByPurchaseIdAsync(int purchaseId, CancellationToken ct)
	{
		return await batchRepo.Query()
			.Where(b => b.PurchaseId == purchaseId)
			.ToListAsync(ct);
	}

	public async Task<bool> HasSoldBatchesAsync(int id, CancellationToken ct)
	{
		return await batchRepo.AnyAsync(b => b.PurchaseId == id && b.Sold > 0, ct);
	}

	public async Task DeleteBatchesByPurchaseIdAsync(int purchaseId, CancellationToken ct)
	{
		await batchRepo.ExecuteDeleteAsync(b => b.PurchaseId == purchaseId, ct);
	}

	public IQueryable<Batch> ActiveBatchesQuery()
	{
		return batchRepo.Query()
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId);
	}

	public async Task<Batch?> GetSingleBatchByIdAsync(int id, CancellationToken ct)
	{
		return await batchRepo.FirstOrDefaultAsync(b => b.Id == id, ct);
	}
}