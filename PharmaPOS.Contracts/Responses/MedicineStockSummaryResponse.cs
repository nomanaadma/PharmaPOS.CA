using PharmaPOS.Domain.BatchAggregate;

namespace PharmaPOS.Contracts.Responses;

public record MedicineStockSummaryResponse(
    IEnumerable<Batch> ActiveBatches, 
    IEnumerable<Batch> ExpiredBatches, 
    Dictionary<int, string> MedicineNames
);
