using ErrorOr;
using PharmaPOS.Domain.Common.ValueObjects;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Helpers;

public static class PricingHelper
{
	public static ErrorOr<PricingDetails> PricingDetails(PricingDetailModel item)
	{
		return PharmaPOS.Domain.Common.ValueObjects.PricingDetails.Create(
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