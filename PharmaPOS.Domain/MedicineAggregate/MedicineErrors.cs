using ErrorOr;

namespace PharmaPOS.Domain.MedicineAggregate;

public static class MedicineErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Medicine.NotFound", $"Medicine {id} does not exist.");

	public static Error NameRequired =>
		Error.Validation("Medicine.NameRequired", "Medicine Name is required.");

	public static Error CompanyRequired =>
		Error.Validation("Medicine.CompanyRequired", "Medicine Company is required.");

	public static Error GenericRequired =>
		Error.Validation("Medicine.GenericRequired", "Medicine Generic is required.");

	public static Error CategoryRequired =>
		Error.Validation("Medicine.CategoryRequired", "A valid Medicine Category is required.");

	public static Error UnitRequired =>
		Error.Validation("Medicine.UnitRequired", "Unit is required.");
}
