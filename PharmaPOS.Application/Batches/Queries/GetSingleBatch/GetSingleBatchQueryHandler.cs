using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Batches.Queries.GetSingleBatch;

public class GetSingleBatchQueryHandler(
	IBatchRepository batchRepo) : IRequestHandler<GetSingleBatchQuery, Batch?>
{
	public async Task<Batch?> Handle(GetSingleBatchQuery query, CancellationToken ct)
	{
		return await batchRepo.GetSingleBatchAsync(query.MedicineId, query.BatchId, ct);
	}
}
