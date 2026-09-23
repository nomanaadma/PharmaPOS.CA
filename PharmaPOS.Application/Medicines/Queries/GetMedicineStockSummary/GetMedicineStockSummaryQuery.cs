using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Medicines.Queries.GetMedicineStockSummary;

public record GetMedicineStockSummaryQuery(int MedicineId) : IRequest<(IEnumerable<BatchResponse> liveBatches, IEnumerable<BatchResponse> badBatches)>;
