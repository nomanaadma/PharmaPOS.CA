using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Mappers;

public static class StockMapper
{
    public static IEnumerable<StockViewModel> MapToStocks(
	    IEnumerable<Batch> batches, 
	    Dictionary<int, string> medicineNames)
    {
	    return batches.Select(batch => new StockViewModel
	    {
		    Id = batch.Id,
		    MedicineId = batch.MedicineId,
		    PurchaseId = batch.PurchaseId,
		    Name = medicineNames.GetValueOrDefault(batch.MedicineId, ""),
		    Batch = batch.BatchName,
		    Expiry = batch.Expiry,
		    Qty = batch.Pricing.Qty,
		    Sold = batch.Sold,
		    Available = batch.Pricing.Qty - batch.Sold,
		    PurchasePrice = batch.PurchasePrice,
		    SalePrice = batch.Pricing.SalePrice
	    });
    }
    
}
