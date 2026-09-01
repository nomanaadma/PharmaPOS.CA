using ErrorOr;

namespace PharmaPOS.Domain.MedicineAggregate;

public static class MedicineErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Medicine.NotFound", $"Medicine {id} does not exist.");
}
