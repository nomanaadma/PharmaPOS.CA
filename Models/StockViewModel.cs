namespace PharmaPOS_MVC.Models;

public class StockViewModel
{
	public int Id { get; init; }
	public int MedicineId { get; init; }
	public int PurchaseId { get; init; }
	public string Name { get; init; } = null!;
	public string Batch { get; init; } = null!;
	public string Expiry { get; init; } = null!;
	public decimal PurchasePrice { get; init; }
	public decimal SalePrice { get; init; }
	public decimal TotalPurchase { get; init; }
	public decimal TotalSales { get; init; }
	public decimal Profit { get; init; }
	public decimal Qty { get; init; }
	public decimal Sold { get; init; }
	public decimal Available { get; init; }
}