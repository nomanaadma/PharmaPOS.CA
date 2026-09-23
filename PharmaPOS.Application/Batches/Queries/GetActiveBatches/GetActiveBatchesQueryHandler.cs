using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Batches.Queries.GetActiveBatches;

public class GetActiveBatchesQueryHandler(
	IBatchRepository batchRepo) : IRequestHandler<GetActiveBatchesQuery, List<Batch>>
{
	public async Task<List<Batch>> Handle(GetActiveBatchesQuery query, CancellationToken ct)
	{
		return await batchRepo.GetActiveBatchesAsync(query.MedicineId, ct);
	}
}
