using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Mappers;

public static class StockMapper
{
	
	public static IEnumerable<StockViewModel> MapToStocksBatch(
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
	
	public static IEnumerable<StockViewModel> MapToStockList(List<StockResponse> result)
	{
		return result.Select(st => new StockViewModel
		{
			Id = st.Id,
			MedicineId = st.MedicineId,
			PurchaseId = st.PurchaseId,
			Name = st.Name,
			Batch = st.Batch,
			Expiry = st.Expiry,
			Qty = st.Qty,
			Sold = st.Sold,
			Available = st.Available,
			PurchasePrice = st.PurchasePrice,
			SalePrice = st.SalePrice,
			Profit = st.Profit,
			TotalPurchase = st.TotalPurchase,
			TotalSales = st.TotalSales,
		});

	}

}