using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Contracts.Responses;

public record PurchaseDetailsResponse(
    Purchase Purchase, 
    List<Batch> Batches, 
    Dictionary<int, string> MedicineNames, 
    Supplier Supplier
);
