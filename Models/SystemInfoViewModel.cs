using System.ComponentModel.DataAnnotations;

namespace PharmaPOS_MVC.Models;

public class SystemInfoViewModel
{
	public string? Logo { get; set; }
	public string? LogoIcon { get; set; }
	public string? LgBackground { get; set; }
	public string? Favicon { get; set; }

	[Required(ErrorMessage = "Store Name is required.")]
	public string Name { get; set; } = null!;

	[Required(ErrorMessage = "Store Legal Name is required.")]
	public string LegalName { get; set; } = null!;

	[Required(ErrorMessage = "Email Address is required.")]
	[EmailAddress(ErrorMessage = "Invalid Email Address.")]
	public string Mail { get; set; } = null!;

	[Required(ErrorMessage = "Phone Number is required.")]
	public string Phone { get; set; } = null!;

	public string? AddressLine1 { get; set; }
	public string? AddressLine2 { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
	public string? Postal { get; set; }
}
