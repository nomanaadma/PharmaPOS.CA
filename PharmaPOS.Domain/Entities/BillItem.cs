using PharmaPOS.Domain.ValueObjects;

namespace PharmaPOS.Domain.Entities;

public class BillItem
{
	// EF Core parameterless constructor
	private BillItem() { }

	public BillItem(
		int billId,
		int medicineId,
		int batchId,
		PricingDetails pricing)
	{
		Update(billId, medicineId, batchId, pricing);
	}

	public int Id { get; private set; }
	public int BillId { get; private set; }
	public Bill Bill { get; private set; } = null!;
	public int MedicineId { get; private set; }
	public Medicine Medicine { get; private set; } = null!;
	public int BatchId { get; private set; }
	public Batch Batch { get; private set; } = null!;
	public PricingDetails Pricing { get; private set; } = null!;

	private void Update(
		int billId,
		int medicineId,
		int batchId,
		PricingDetails pricing)
	{
		if (medicineId <= 0)
			throw new ArgumentException("Valid Medicine is required.", nameof(medicineId));
		if (batchId <= 0)
			throw new ArgumentException("Valid Batch is required.", nameof(batchId));
		if (pricing == null)
			throw new ArgumentNullException(nameof(pricing));

		BillId = billId;
		MedicineId = medicineId;
		BatchId = batchId;
		Pricing = pricing;
	}
}
