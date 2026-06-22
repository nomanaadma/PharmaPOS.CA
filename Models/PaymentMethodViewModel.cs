using System.ComponentModel.DataAnnotations;
using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Models;

public class PaymentMethodViewModel
{
	public int? Id { get; init; }

	[Required(ErrorMessage = "Payment Method Name is required.")]
	[StringLength(100, ErrorMessage = "Payment Method Name cannot exceed 100 characters.")]
	public string Name { get; init; } = null!;

	[Required(ErrorMessage = "Status is required.")]
	[EnumDataType(typeof(Status), ErrorMessage = "Invalid status value.")]
	public Status Status { get; init; } = Status.Active;
}
