using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaPOS.MVC.Models;

public class PurchaseFormModel
{
	public int? Id { get; init; }

	[Required(ErrorMessage = "Supplier is required.")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a valid supplier.")]
	public int Supplier { get; init; }

	[Required(ErrorMessage = "Date is required.")]
	// Set this to match EXACTLY what your bootstrap-datetimepicker outputs
	[DataType(DataType.Date)]
	[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime Date { get; init; } = DateTime.Now;
	public string? Note { get; init; }
	public decimal Subtotal { get; init; }
	public decimal Tax { get; init; }
	public decimal Discount { get; init; }
	public decimal Amount { get; init; }
	public List<PurchaseItemFormModel> Items { get; init; } = [];
	
	public IEnumerable<SelectListItem> Suppliers { get; set; } = [];
}

public class PurchaseItemFormModel : PricingDetailModel
{
	public string Batch { get; init; } = null!;
	public string? Pqty { get; init; }
	public decimal Purchaseprice { get; init; }
}
