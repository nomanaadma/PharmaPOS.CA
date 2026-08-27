using ErrorOr;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Domain.Common.ValueObjects;

namespace PharmaPOS.Application.Helpers;

public static class PricingHelper
{
	public static ErrorOr<PricingDetails> CreatePricingDetails(PricingDetailRequest item)
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