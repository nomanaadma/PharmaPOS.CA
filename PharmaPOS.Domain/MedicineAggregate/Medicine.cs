using ErrorOr;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.MedicineCategoryAggregate;

namespace PharmaPOS.Domain.MedicineAggregate;

public class Medicine : AggregateRoot
{
	
	// EF Core parameterless constructor
	private Medicine() { }

	private Medicine(MedicineDetails details)
	{
		SetDetails(details);
	}

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
	public DateTime CreatedDate { get; private init; }
	
	/// <summary>
	/// Factory method: validates inputs and returns an ErrorOr&lt;Medicine&gt;.
	/// </summary>
	public static ErrorOr<Medicine> Create(MedicineDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Medicine(details);
	}

	/// <summary>
	/// Updates the medicine details. Returns ErrorOr&lt;Updated&gt; with validation.
	/// </summary>
	public ErrorOr<Updated> Update(MedicineDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}

	private void SetDetails(MedicineDetails details)
	{
		Name = details.Name;
		Company = details.Company;
		Generic = details.Generic;
		MedicineGroup = details.MedicineGroup;
		CategoryId = details.CategoryId;
		StoreBox = details.StoreBox;
		MinLevel = details.MinLevel;
		ReOrderLevel = details.ReOrderLevel;
		Unit = details.Unit;
		UnitPacking = details.UnitPacking;
		Note = details.Note;
	}

	private static List<Error> ValidateDetails(MedicineDetails details)
	{
		var errors = new List<Error>();

		if (string.IsNullOrWhiteSpace(details.Name))
			errors.Add(MedicineErrors.NameRequired);
		if (string.IsNullOrWhiteSpace(details.Company))
			errors.Add(MedicineErrors.CompanyRequired);
		if (string.IsNullOrWhiteSpace(details.Generic))
			errors.Add(MedicineErrors.GenericRequired);
		if (details.CategoryId <= 0)
			errors.Add(MedicineErrors.CategoryRequired);
		if (string.IsNullOrWhiteSpace(details.Unit))
			errors.Add(MedicineErrors.UnitRequired);

		return errors;
	}
}
