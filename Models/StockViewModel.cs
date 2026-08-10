namespace PharmaPOS_MVC.Models;

public class StockViewModel
{
	public int Id { get; init; }
	public int MedicineId { get; init; }
	public int PurchaseId { get; init; }
	public string Name { get; init; } = null!;
	public string Batch { get; init; } = null!;
	public string Expiry { get; init; } = null!;
	public decimal PurchasePrice { get; set; }
	public decimal SalePrice { get; set; }
	public decimal TotalPurchase { get; set; }
	public decimal TotalSales { get; set; }
	public decimal Profit { get; set; }
	public decimal Qty { get; init; }
	public decimal Sold { get; init; }
	public decimal Available { get; init; }
}