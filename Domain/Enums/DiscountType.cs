using System.ComponentModel.DataAnnotations;

namespace PharmaPOS_MVC.Domain.Enums;

public enum DiscountType
{
	[Display(Name = "%")]
	Percentage = 1,
	
	[Display(Name = "Flat")]
	Flat = 2,
}