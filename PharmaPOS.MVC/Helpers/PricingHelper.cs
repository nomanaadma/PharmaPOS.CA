using PharmaPOS.Domain.Enums;
using PharmaPOS.Domain.ValueObjects;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Helpers;

public static class PricingHelper
{
	public static PricingDetails PricingDetails(PricingDetailModel item)
	{
		return new PricingDetails(
			item.Qty, 
			item.SalePrice, 
			item.Gross, 
			item.DiscountType, 
			item.Discount, 
			item.DiscountValue, 
			item.Tax, 
			item.TaxPrice, 
			item.Price
		);
	}
}