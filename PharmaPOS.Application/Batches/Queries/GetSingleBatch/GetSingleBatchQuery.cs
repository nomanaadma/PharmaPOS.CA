using MediatR;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Batches.Queries.GetSingleBatch;

public record GetSingleBatchQuery(int MedicineId, int BatchId) : IRequest<Batch?>;
