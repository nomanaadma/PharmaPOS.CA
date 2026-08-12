using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class StockMapper
{
   
    public static IEnumerable<StockViewModel> MapToStocks(IEnumerable<Batch> batches)
    {
	    return batches.Select(batch => MapToStock(batch));
    }

    public static StockViewModel MapToStock(Batch batch,
	    bool? single = true,
	    decimal? unitPurchasePrice = null,
	    decimal? unitSalePrice = null,
	    decimal? totalPurchase = null,
	    decimal? totalSales = null,
	    decimal? profit = null
	    )
    {
	    
	    var stockViewModel =  new StockViewModel
	    {
		    Id = batch.Id,
		    MedicineId = batch.MedicineId,
		    PurchaseId = batch.PurchaseId,
		    Name = batch.Medicine.Name,
		    Batch = batch.BatchName,
		    Expiry = batch.Expiry,
		    Qty = batch.Pricing.Qty,
		    Sold = batch.Sold,
		    Available = batch.Pricing.Qty - batch.Sold
	    };

	    if (single == true)
	    {
		    stockViewModel.PurchasePrice = batch.PurchasePrice;
		    stockViewModel.SalePrice = batch.Pricing.SalePrice;
	    }
	    else
	    {
		    
		    if (unitPurchasePrice != null)
		    {
			    stockViewModel.PurchasePrice = unitPurchasePrice.Value;
		    }
		    if (unitSalePrice != null)
		    {
			    stockViewModel.SalePrice = unitSalePrice.Value;
		    }
		    if (totalPurchase != null)
		    {
			    stockViewModel.TotalPurchase = totalPurchase.Value;
		    }
		    if (totalSales != null)
		    {
			    stockViewModel.TotalSales = totalSales.Value;
		    }
		    if (profit != null)
		    {
			    stockViewModel.Profit = profit.Value;
		    }    
		    
	    }

	    return stockViewModel;

    }
}
