using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.Common.ValueObjects;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Domain.PurchaseAggregate;

public class Batch : EntityWithDate
{
	// EF Core parameterless constructor
	private Batch() { }

	public Batch(BatchDetails details)
	{
		Update(details);
	}

	public string BatchName { get; private set; } = null!;
	public string Expiry { get; private set; } = null!;
	public string? Pqty { get; private set; }
	public PricingDetails Pricing { get; private set; } = null!;
	public decimal PurchasePrice { get; private set; }
	public decimal Sold { get; private set; }
	public int MedicineId { get; private set; }
	public Medicine Medicine { get; private set; } = null!;
	public int PurchaseId { get; private set; }
	public Purchase Purchase { get; private set; } = null!;
	public Status Status { get; private set; } = Status.Active;

	public void Update(BatchDetails details)
	{

		BatchName = details.BatchName;
		Expiry = details.Expiry;
		Pqty = details.Pqty;
		Pricing = details.Pricing;
		PurchasePrice = details.PurchasePrice;
		MedicineId = details.MedicineId;
		PurchaseId = details.PurchaseId;
	}

	public void UpdateSold(decimal sold)
	{
		Sold = sold;
	}

	public void UpdateStatus(Status status)
	{
		Status = status;
	}
}
