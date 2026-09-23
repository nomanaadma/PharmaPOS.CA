using MediatR;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Batches.Queries.GetActiveBatches;

public record GetActiveBatchesQuery(int MedicineId) : IRequest<List<Batch>>;