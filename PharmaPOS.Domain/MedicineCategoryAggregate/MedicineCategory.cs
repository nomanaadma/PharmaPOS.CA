using ErrorOr;
using PharmaPOS.Domain.Common;

namespace PharmaPOS.Domain.MedicineCategoryAggregate;

public class MedicineCategory : AggregateRoot
{
	// EF Core parameterless constructor
	private MedicineCategory() { }

	private MedicineCategory(string name)
	{
		Name = name;
	}

	public string Name { get; private set; } = null!;

	public static ErrorOr<MedicineCategory> Create(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return NameRequired;

		return new MedicineCategory(name);
	}

	public ErrorOr<Updated> Update(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return NameRequired;

		Name = name;
		return Result.Updated;
	}
	
	private static Error NameRequired =>
		Error.Validation("Category.NameRequired", "Category Name is required.");
	
}
