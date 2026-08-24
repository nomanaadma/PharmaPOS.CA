using ErrorOr;

namespace PharmaPOS.Domain.MedicineCategoryAggregate;

public static class CategoryErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Category.NotFound", $"Category {id} does not exist.");

	public static Error NameRequired =>
		Error.Validation("Category.NameRequired", "Category Name is required.");

	public static Error NameNotUnique =>
		Error.Conflict("Category.NameNotUnique", "Category Name must be unique.");
}
