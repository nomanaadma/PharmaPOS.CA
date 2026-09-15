using ErrorOr;

namespace PharmaPOS.Domain.CategoryAggregate;

public static class CategoryErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Category.NotFound", $"Category {id} does not exist.");
	

	public static readonly Error NameNotUnique =
		Error.Conflict("Category.NameNotUnique", "Category Name must be unique.");
}
