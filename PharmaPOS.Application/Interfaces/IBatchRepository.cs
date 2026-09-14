using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Interfaces;

public interface IBatchRepository : IRepository<Batch>
{
	Task<List<Batch>> GetNonExpiredBatchesAsync(List<int> medicineIds, CancellationToken ct = default);
	Task<List<Batch>> GetNonExpiredBatchesForMedicineAsync(int medicineId, CancellationToken ct = default);
	Task<List<Batch>> GetActiveBatchesByMedicineAsync(int medicineId, CancellationToken ct = default);
	Task<Batch?> GetSingleBatchAsync(int medicineId, int batchId, CancellationToken ct = default);
	Task<Dictionary<int, Batch>> GetBatchByIdsAsync(List<int> batchIds, CancellationToken ct = default);
	Task<List<Batch>> GetBatchesByPurchaseIdAsync(int purchaseId, CancellationToken ct = default);
	Task<bool> HasSoldBatchesAsync(int purchaseId, CancellationToken ct = default);
	Task DeleteBatchesByPurchaseIdAsync(int purchaseId, CancellationToken ct = default);
	Task<List<Batch>> GetActiveBatchesAsync(string? expiryFilterType = null, CancellationToken ct = default);
	Task AddRangeAsync(IEnumerable<Batch> batches, CancellationToken ct = default);
	void RemoveRange(IEnumerable<Batch> batches);
}
