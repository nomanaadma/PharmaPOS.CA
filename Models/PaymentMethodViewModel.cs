using System.ComponentModel.DataAnnotations;

namespace PharmaPOS_MVC.Models;

public class PaymentMethodViewModel
{
	public int? Id { get; init; }

	[Required(ErrorMessage = "Payment Method Name is required.")]
	[StringLength(100, ErrorMessage = "Payment Method Name cannot exceed 100 characters.")]
	public string Name { get; init; } = null!;

	[Required(ErrorMessage = "Status is required.")]
	[Range(0, 1, ErrorMessage = "Status must be Active (1) or InActive (0).")]
	public int Status { get; init; } = 1; // Default to Active
}
