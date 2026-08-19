using PharmaPOS.Domain.Enums;
using PharmaPOS.Domain.ValueObjects;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Helpers;

public static class PricingHelper
{
	public static PricingDetails PricingDetails(PricingDetailModel item)
	{
		return new PricingDetails(
			item.Qty, 
			item.Saleprice, 
			item.Gross, 
			item.Discounttype, 
			item.Discount, 
			item.Discountvalue, 
			item.Tax, 
			item.Taxprice, 
			item.Price
		);
	}
}