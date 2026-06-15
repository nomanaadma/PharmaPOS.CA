using System;

namespace PharmaPOS_MVC.Domain.Entities;

public class MedicineCategory
{
	// EF Core parameterless constructor
	private MedicineCategory() { }

	public MedicineCategory(string name)
	{
		Update(name);
	}

	public int Id { get; private set; }
	public string Name { get; private set; } = null!;

	public void Update(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Category Name is required.", nameof(name));

		Name = name;
	}
}
