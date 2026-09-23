using MediatR;
using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Application.Batches.Queries.GetNonExpiredBatches;

public record GetNonExpiredBatchesQuery(int MedicineId) : IRequest<List<Batch>>;
