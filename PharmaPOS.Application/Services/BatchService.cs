using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Services;

public class BatchService(IBatchRepository batchRepo) : IApplicationService
{

	public async Task<List<Batch>> GetNonExpiredBatchesForMedicineAsync(int medicineId, CancellationToken ct = default)
	{
		return await batchRepo.GetNonExpiredBatchesForMedicineAsync(medicineId, ct);
	}

	public async Task<List<Batch>> GetActiveBatchesByMedicineAsync(int medicineId, CancellationToken ct = default)
	{
		return await batchRepo.GetActiveBatchesByMedicineAsync(medicineId, ct);
	}

	public async Task<Batch?> GetSingleBatchAsync(int medicineId, int batchId, CancellationToken ct = default)
	{
		return await batchRepo.GetSingleBatchAsync(medicineId, batchId, ct);
	}
	
}