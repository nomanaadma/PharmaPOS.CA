using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Models;

public class PricingDetailModel
{
	public int? Id { get; init; }
	public int MedicineId { get; init; }
	public string Name { get; init; } = null!;
	public string Expiry { get; init; } = null!;
	public decimal Qty { get; init; }
	public decimal Saleprice { get; init; }
	public decimal Gross { get; init; }
	public DiscountType Discounttype { get; init; }
	public decimal Discount { get; init; }
	public decimal Discountvalue { get; init; }
	public decimal Tax { get; init; }
	public decimal Taxprice { get; init; }
	public decimal Price { get; init; }
}