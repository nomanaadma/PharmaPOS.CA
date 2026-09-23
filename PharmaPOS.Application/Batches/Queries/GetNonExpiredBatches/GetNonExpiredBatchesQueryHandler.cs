using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Batches.Queries.GetNonExpiredBatches;

public class GetNonExpiredBatchesQueryHandler(
	IBatchRepository batchRepo) : IRequestHandler<GetNonExpiredBatchesQuery, List<Batch>>
{
	public async Task<List<Batch>> Handle(GetNonExpiredBatchesQuery query, CancellationToken ct)
	{
		return await batchRepo.GetNonExpiredBatchesAsync(query.MedicineId, ct);
	}
}
