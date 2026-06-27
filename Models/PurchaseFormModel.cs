using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaPOS_MVC.Models;

public class PurchaseFormModel
{
	public int? Id { get; set; }

	[Required(ErrorMessage = "Supplier is required.")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a valid supplier.")]
	public int Supplier { get; set; }
	
	[Required(ErrorMessage = "Date is required.")]
	public string Date { get; set; } = null!;
	public string? Note { get; set; }
	public decimal Total { get; set; }
	public decimal Tax { get; set; }
	public decimal Discount { get; set; }
	public decimal Amount { get; set; }
	public List<PurchaseItemFormModel> Items { get; set; } = [];
	
	public IEnumerable<SelectListItem> Suppliers { get; set; } = [];
}

public class PurchaseItemFormModel
{
	public int? Id { get; set; }
	public int Medicine_Id { get; set; }
	public string Name { get; set; } = null!;
	public string Batch { get; set; } = null!;
	public string Expiry { get; set; } = null!;
	public string? Pqty { get; set; }
	public decimal Qty { get; set; }
	public decimal Purchaseprice { get; set; }
	public decimal Saleprice { get; set; }
	public decimal Gross { get; set; }
	public int Discounttype { get; set; }
	public decimal Discount { get; set; }
	public decimal Discountvalue { get; set; }
	public decimal Tax { get; set; }
	public decimal Taxprice { get; set; }
	public decimal Price { get; set; }
}
