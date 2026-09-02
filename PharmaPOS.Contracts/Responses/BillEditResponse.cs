using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.BillingAggregate;

namespace PharmaPOS.Contracts.Responses;

public record BillEditResponse(
    Bill Bill, 
    Dictionary<int, List<Batch>> BatchesByMedicine, 
    Dictionary<int, string> MedicineNames
);
