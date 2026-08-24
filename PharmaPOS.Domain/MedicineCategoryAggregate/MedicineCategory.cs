using ErrorOr;
using PharmaPOS.Domain.Common.Entities;

namespace PharmaPOS.Domain.MedicineCategoryAggregate;

public class MedicineCategory : Entity
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
			return CategoryErrors.NameRequired;

		return new MedicineCategory(name);
	}

	public ErrorOr<Updated> Update(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return CategoryErrors.NameRequired;

		Name = name;
		return Result.Updated;
	}
}
