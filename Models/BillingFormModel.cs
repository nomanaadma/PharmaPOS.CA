using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaPOS_MVC.Models;

public class BillingFormModel
{
	public int? Id { get; init; }

	public int? CustomerId { get; init; }

	[Required(ErrorMessage = "Name is required.")]
	public string Name { get; init; } = null!;

	[EmailAddress(ErrorMessage = "Invalid Email Address")]
	public string? Email { get; init; }

	public string? Mobile { get; init; }

	[Required(ErrorMessage = "Payment Method is required.")]
	public int Method { get; init; }

	[Required(ErrorMessage = "Date is required.")]
	[DataType(DataType.Date)]
	[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime BillDate { get; init; } = DateTime.Now;

	public string? Note { get; init; }
	public decimal Subtotal { get; init; }
	public decimal Tax { get; init; }
	public decimal DiscountValue { get; init; }
	public decimal Amount { get; init; }
	public List<BillingItemFormModel> Items { get; init; } = [];

	public IEnumerable<SelectListItem> PaymentMethods { get; set; } = [];
}

public class BillingItemFormModel
{
	public int? Id { get; init; }
	public int MedicineId { get; init; }
	public string Name { get; init; } = null!;
	public int Batch { get; init; }
	public string BatchName { get; init; } = null!;
	public string Expiry { get; init; } = null!;
	public decimal Qty { get; init; }
	public decimal Saleprice { get; init; }
	public decimal Gross { get; init; }
	public int Discounttype { get; init; }
	public decimal Discount { get; init; }
	public decimal Discountvalue { get; init; }
	public decimal Tax { get; init; }
	public decimal Taxprice { get; init; }
	public decimal Price { get; init; }
}
