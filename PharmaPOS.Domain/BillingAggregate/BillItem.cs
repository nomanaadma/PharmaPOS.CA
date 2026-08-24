using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.ValueObjects;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Domain.PurchaseAggregate;

namespace PharmaPOS.Domain.BillingAggregate;

public class BillItem : Entity
{
	// EF Core parameterless constructor
	private BillItem() { }

	public BillItem(BillItemDetails details)
	{
		Update(details);
	}

	public int BillId { get; private set; }
	public Bill Bill { get; private set; } = null!;
	public int MedicineId { get; private set; }
	public Medicine Medicine { get; private set; } = null!;
	public int BatchId { get; private set; }
	public Batch Batch { get; private set; } = null!;
	public PricingDetails Pricing { get; private set; } = null!;

	private void Update(BillItemDetails details)
	{
		BillId = details.BillId;
		MedicineId = details.MedicineId;
		BatchId = details.BatchId;
		Pricing = details.Pricing;
	}
}
