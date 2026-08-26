using ErrorOr;
using PharmaPOS.Domain.Common.ValueObjects;

namespace PharmaPOS.Application.Helpers;

public static class PricingHelper
{
	public static ErrorOr<PricingDetails> CreatePricingDetails(PricingDetailModel item)
	{
		return PricingDetails.Create(
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