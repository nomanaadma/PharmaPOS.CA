namespace PharmaPOS_MVC.Domain.Entities;

public class BillItem
{
	// EF Core parameterless constructor
	private BillItem() { }

	public BillItem(
		int billId,
		int medicineId,
		int batchId,
		decimal qty,
		decimal salePrice,
		decimal gross,
		int discountType,
		decimal discount,
		decimal discountValue,
		decimal tax,
		decimal taxPrice,
		decimal price)
	{
		Update(billId, medicineId, batchId, qty, salePrice, gross, discountType, discount, discountValue, tax, taxPrice, price);
	}

	public int Id { get; private set; }
	public int BillId { get; private set; }
	public MedicineBill Bill { get; private set; } = null!;
	public int MedicineId { get; private set; }
	public Medicine Medicine { get; private set; } = null!;
	public int BatchId { get; private set; }
	public MedicineBatch Batch { get; private set; } = null!;
	public decimal Qty { get; private set; }
	public decimal SalePrice { get; private set; }
	public decimal Gross { get; private set; }
	public int DiscountType { get; private set; }
	public decimal Discount { get; private set; }
	public decimal DiscountValue { get; private set; }
	public decimal Tax { get; private set; }
	public decimal TaxPrice { get; private set; }
	public decimal Price { get; private set; }

	public void Update(
		int billId,
		int medicineId,
		int batchId,
		decimal qty,
		decimal salePrice,
		decimal gross,
		int discountType,
		decimal discount,
		decimal discountValue,
		decimal tax,
		decimal taxPrice,
		decimal price)
	{
		if (medicineId <= 0)
			throw new ArgumentException("Valid Medicine is required.", nameof(medicineId));
		if (batchId <= 0)
			throw new ArgumentException("Valid Batch is required.", nameof(batchId));
		if (qty < 0)
			throw new ArgumentException("Quantity cannot be negative.", nameof(qty));
		if (salePrice < 0)
			throw new ArgumentException("Sale Price cannot be negative.", nameof(salePrice));

		BillId = billId;
		MedicineId = medicineId;
		BatchId = batchId;
		Qty = qty;
		SalePrice = salePrice;
		Gross = gross;
		DiscountType = discountType;
		Discount = discount;
		DiscountValue = discountValue;
		Tax = tax;
		TaxPrice = taxPrice;
		Price = price;
	}
}
