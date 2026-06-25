using System;

namespace PharmaPOS_MVC.Models;

public class PurchaseViewModel
{
	public int Id { get; set; }
	public string SupplierName { get; set; } = null!;
	public decimal Total { get; set; }
	public decimal Tax { get; set; }
	public decimal DiscountValue { get; set; }
	public decimal Amount { get; set; }
	public DateTime Date { get; set; }
	public string? Note { get; set; }
}
