using System;
using PharmaPOS.Domain.Enums;
using PharmaPOS.Domain.ValueObjects;

namespace PharmaPOS.Domain.Entities;

public class Batch : EntityWithDate
{
	// EF Core parameterless constructor
	private Batch() { }

	public Batch(
		string batch,
		string expiry,
		string? pqty,
		PricingDetails pricing,
		decimal purchasePrice,
		int medicineId,
		int purchaseId)
	{
		Update(batch, expiry, pqty, pricing, purchasePrice, medicineId, purchaseId);
		Sold = 0;
		Status = Status.Active;
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
	public Status Status { get; private set; }

	public void Update(
		string batch,
		string expiry,
		string? pqty,
		PricingDetails pricing,
		decimal purchasePrice,
		int medicineId,
		int purchaseId)
	{
		if (string.IsNullOrWhiteSpace(batch))
			throw new ArgumentException("Batch No is required.", nameof(batch));
		if (string.IsNullOrWhiteSpace(expiry))
			throw new ArgumentException("Expiry Date is required.", nameof(expiry));
		if (pricing == null)
			throw new ArgumentNullException(nameof(pricing));
		if (purchasePrice < 0)
			throw new ArgumentException("Purchase Price cannot be negative.", nameof(purchasePrice));
		if (medicineId <= 0)
			throw new ArgumentException("Valid Medicine is required.", nameof(medicineId));

		BatchName = batch;
		Expiry = expiry;
		Pqty = pqty;
		Pricing = pricing;
		PurchasePrice = purchasePrice;
		MedicineId = medicineId;
		PurchaseId = purchaseId;
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
