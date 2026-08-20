using System;

namespace PharmaPOS.Domain.Entities;

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
