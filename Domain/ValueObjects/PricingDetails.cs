using System;

namespace PharmaPOS_MVC.Domain.ValueObjects;

public record PricingDetails
{
	// EF Core parameterless constructor
	private PricingDetails() { }

	public PricingDetails(
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
		if (qty < 0)
			throw new ArgumentException("Quantity cannot be negative.", nameof(qty));
		if (salePrice < 0)
			throw new ArgumentException("Sale Price cannot be negative.", nameof(salePrice));

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

	public decimal Qty { get; private set; }
	public decimal SalePrice { get; private set; }
	public decimal Gross { get; private set; }
	public int DiscountType { get; private set; }
	public decimal Discount { get; private set; }
	public decimal DiscountValue { get; private set; }
	public decimal Tax { get; private set; }
	public decimal TaxPrice { get; private set; }
	public decimal Price { get; private set; }
}
