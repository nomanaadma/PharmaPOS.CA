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
		if (string.IsNullOrWhiteSpace(details.BatchName))
			throw new ArgumentException("Batch No is required.", nameof(details.BatchName));
		if (string.IsNullOrWhiteSpace(details.Expiry))
			throw new ArgumentException("Expiry Date is required.", nameof(details.Expiry));
		if (details.Pricing == null)
			throw new ArgumentNullException(nameof(details.Pricing));
		if (details.PurchasePrice < 0)
			throw new ArgumentException("Purchase Price cannot be negative.", nameof(details.PurchasePrice));
		if (details.MedicineId <= 0)
			throw new ArgumentException("Valid Medicine is required.", nameof(details.MedicineId));

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
		if (sold < 0)
			throw new ArgumentException("Sold quantity cannot be negative.", nameof(sold));
		Sold = sold;
	}

	public void UpdateStatus(Status status)
	{
		Status = status;
	}
}
