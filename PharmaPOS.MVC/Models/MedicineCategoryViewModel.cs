using System.ComponentModel.DataAnnotations;

namespace PharmaPOS.MVC.Models;

public class MedicineCategoryViewModel
{
	public int? Id { get; init; }

	[Required(ErrorMessage = "Category Name is required.")]
	[StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
	public string Name { get; init; } = null!;
}
