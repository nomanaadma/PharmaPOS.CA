
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Contracts.Responses;

public record InventoryReportResponse(
    Medicine Medicine,
    decimal TotalPurchase,
    decimal TotalSales,
    decimal LiveStock
);
