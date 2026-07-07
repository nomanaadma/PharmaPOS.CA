using System;
using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Domain.Entities;

public class MedicineBatch
{
	// EF Core parameterless constructor
	private MedicineBatch() { }

	public MedicineBatch(
		string batch,
		string expiry,
		string? pqty,
		decimal qty,
		decimal salePrice,
		decimal purchasePrice,
		decimal gross,
		int discountType,
		decimal discount,
		decimal discountValue,
		decimal tax,
		decimal taxPrice,
		decimal price,
		int medicineId,
		int purchaseId)
	{
		Update(batch, expiry, pqty, qty, salePrice, purchasePrice, gross, discountType, discount, discountValue, tax, taxPrice, price, medicineId, purchaseId);
		CreatedDate = DateTime.UtcNow;
		Sold = 0;
		Status = Status.Active;
	}

	public int Id { get; private set; }
	public string Batch { get; private set; } = null!;
	public string Expiry { get; private set; } = null!;
	public string? Pqty { get; private set; }
	public decimal Qty { get; private set; }
	public decimal SalePrice { get; private set; }
	public decimal PurchasePrice { get; private set; }
	public decimal Gross { get; private set; }
	public int DiscountType { get; private set; }
	public decimal Discount { get; private set; }
	public decimal DiscountValue { get; private set; }
	public decimal Tax { get; private set; }
	public decimal TaxPrice { get; private set; }
	public decimal Price { get; private set; }
	public decimal Sold { get; private set; }
	public int MedicineId { get; private set; }
	public Medicine Medicine { get; private set; } = null!;
	public int PurchaseId { get; private set; }
	public Purchase Purchase { get; private set; } = null!;
	public Status Status { get; private set; }
	public DateTime CreatedDate { get; private set; }

	public void Update(
		string batch,
		string expiry,
		string? pqty,
		decimal qty,
		decimal salePrice,
		decimal purchasePrice,
		decimal gross,
		int discountType,
		decimal discount,
		decimal discountValue,
		decimal tax,
		decimal taxPrice,
		decimal price,
		int medicineId,
		int purchaseId)
	{
		if (string.IsNullOrWhiteSpace(batch))
			throw new ArgumentException("Batch No is required.", nameof(batch));
		if (string.IsNullOrWhiteSpace(expiry))
			throw new ArgumentException("Expiry Date is required.", nameof(expiry));
		if (qty < 0)
			throw new ArgumentException("Quantity cannot be negative.", nameof(qty));
		if (salePrice < 0)
			throw new ArgumentException("Sale Price cannot be negative.", nameof(salePrice));
		if (purchasePrice < 0)
			throw new ArgumentException("Purchase Price cannot be negative.", nameof(purchasePrice));
		if (medicineId <= 0)
			throw new ArgumentException("Valid Medicine is required.", nameof(medicineId));

		Batch = batch;
		Expiry = expiry;
		Pqty = pqty;
		Qty = qty;
		SalePrice = salePrice;
		PurchasePrice = purchasePrice;
		Gross = gross;
		DiscountType = discountType;
		Discount = discount;
		DiscountValue = discountValue;
		Tax = tax;
		TaxPrice = taxPrice;
		Price = price;
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
