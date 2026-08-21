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
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Name);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Company);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Generic);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.CategoryId);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Unit);

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
