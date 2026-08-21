using System;
using PharmaPOS.Domain.Common.Entities;

namespace PharmaPOS.Domain.MedicineCategoryAggregate;

public class MedicineCategory : Entity
{
	// EF Core parameterless constructor
	private MedicineCategory() { }

	public MedicineCategory(string name)
	{
		Update(name);
	}

	public string Name { get; private set; } = null!;

	public void Update(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Category Name is required.", nameof(name));

		Name = name;
	}
}
