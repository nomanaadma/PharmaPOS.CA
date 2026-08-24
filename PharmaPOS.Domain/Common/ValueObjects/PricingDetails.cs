using ErrorOr;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Domain.Common.ValueObjects;

public record PricingDetails
{
	// EF Core parameterless constructor
	private PricingDetails() { }

	private PricingDetails(
		decimal qty,
		decimal salePrice,
		decimal gross,
		DiscountType discountType,
		decimal discount,
		decimal discountValue,
		decimal tax,
		decimal taxPrice,
		decimal price)
	{
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

	public static ErrorOr<PricingDetails> Create(
		decimal qty,
		decimal salePrice,
		decimal gross,
		DiscountType discountType,
		decimal discount,
		decimal discountValue,
		decimal tax,
		decimal taxPrice,
		decimal price)
	{
		if (qty < 0)
			return PricingErrors.NegativeQuantity;
		if (salePrice < 0)
			return PricingErrors.NegativeSalePrice;

		return new PricingDetails(qty, salePrice, gross, discountType, discount, discountValue, tax, taxPrice, price);
	}



	public decimal Qty { get; private set; }
	public decimal SalePrice { get; private set; }
	public decimal Gross { get; private set; }
	public DiscountType DiscountType { get; private set; }
	public decimal Discount { get; private set; }
	public decimal DiscountValue { get; private set; }
	public decimal Tax { get; private set; }
	public decimal TaxPrice { get; private set; }
	public decimal Price { get; private set; }
}
