using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.MedicineCategoryAggregate;
using PharmaPOS.Domain.PurchaseAggregate;


namespace PharmaPOS.Domain.MedicineAggregate;

public class Medicine : EntityWithDate
{
	
	// EF Core parameterless constructor
	private Medicine() { }

	public Medicine(MedicineDetails details)
	{
		Update(details);
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
	
	public ICollection<Batch> Batches { get; init; } = [];
	
	public void Update(MedicineDetails details)
	{
		if (string.IsNullOrWhiteSpace(details.Name))
			throw new ArgumentException("Medicine Name is required.", nameof(details.Name));
		if (string.IsNullOrWhiteSpace(details.Company))
			throw new ArgumentException("Medicine Company is required.", nameof(details.Company));
		if (string.IsNullOrWhiteSpace(details.Generic))
			throw new ArgumentException("Medicine Generic is required.", nameof(details.Generic));
		if (details.CategoryId <= 0)
			throw new ArgumentException("Valid Medicine Category is required.", nameof(details.CategoryId));
		if (string.IsNullOrWhiteSpace(details.Unit))
			throw new ArgumentException("Unit is required.", nameof(details.Unit));

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
}
