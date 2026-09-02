using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.BillingAggregate;

namespace PharmaPOS.Contracts.Responses;

public record BillDetailsResponse(
    Bill Bill, 
    Dictionary<int, Batch> BatchesById, 
    Dictionary<int, string> MedicineNames, 
    string PaymentMethodName
);
