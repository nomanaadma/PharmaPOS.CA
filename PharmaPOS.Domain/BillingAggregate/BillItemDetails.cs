using PharmaPOS.Domain.Common.ValueObjects;

namespace PharmaPOS.Domain.BillingAggregate;

public record BillItemDetails(int BillId, int MedicineId, int BatchId, PricingDetails Pricing);
