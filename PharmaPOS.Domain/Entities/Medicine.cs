using System;

namespace PharmaPOS.Domain.Entities;

public class Medicine
{
	
	// EF Core parameterless constructor
	private Medicine() { }

	public Medicine(
		string name,
		string company,
		string generic,
		string? medicineGroup,
		int categoryId,
		string? storeBox,
		decimal? minLevel,
		decimal? reOrderLevel,
		string unit,
		string? unitPacking,
		string? note)
	{
		Update(name, company, generic, medicineGroup, categoryId, storeBox, minLevel, reOrderLevel, unit, unitPacking, note);
		CreatedDate = DateTime.UtcNow;
	}

	public int Id { get; private set; }
	public string Name { get; private set; } = null!;
	public string Company { get; private set; } = null!;
	public string Generic { get; private set; } = null!;
	public string? MedicineGroup { get; private set; }
	public int CategoryId { get; private set; }
	public MedicineCategory Category { get; private set; } = null!;
	public string? StoreBox { get; private set; }
	public decimal? MinLevel { get; private set; }
	public decimal? ReOrderLevel { get; private set; }
	public string Unit { get; private set; } = null!;
	public string? UnitPacking { get; private set; }
	public string? Note { get; private set; }
	public DateTime CreatedDate { get; private set; }
	
	public ICollection<Batch> Batches { get; init; } = [];
	
	public void Update(
		string name,
		string company,
		string generic,
		string? medicineGroup,
		int categoryId,
		string? storeBox,
		decimal? minLevel,
		decimal? reOrderLevel,
		string unit,
		string? unitPacking,
		string? note)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Medicine Name is required.", nameof(name));
		if (string.IsNullOrWhiteSpace(company))
			throw new ArgumentException("Medicine Company is required.", nameof(company));
		if (string.IsNullOrWhiteSpace(generic))
			throw new ArgumentException("Medicine Generic is required.", nameof(generic));
		if (categoryId <= 0)
			throw new ArgumentException("Valid Medicine Category is required.", nameof(categoryId));
		if (string.IsNullOrWhiteSpace(unit))
			throw new ArgumentException("Unit is required.", nameof(unit));

		Name = name;
		Company = company;
		Generic = generic;
		MedicineGroup = medicineGroup;
		CategoryId = categoryId;
		StoreBox = storeBox;
		MinLevel = minLevel;
		ReOrderLevel = reOrderLevel;
		Unit = unit;
		UnitPacking = unitPacking;
		Note = note;
	}
}
