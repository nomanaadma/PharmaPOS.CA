using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Contracts.Responses;

public record MedicineStockResponse(Medicine Medicine, decimal LiveStock);