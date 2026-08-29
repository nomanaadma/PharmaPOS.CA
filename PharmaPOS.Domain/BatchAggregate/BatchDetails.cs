using PharmaPOS.Domain.Common.ValueObjects;

namespace PharmaPOS.Domain.BatchAggregate;

public record BatchDetails(string BatchName, string Expiry, string? Pqty, PricingDetails Pricing, decimal PurchasePrice, int MedicineId, int PurchaseId);
