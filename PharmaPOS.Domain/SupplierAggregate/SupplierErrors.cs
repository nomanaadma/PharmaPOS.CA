using ErrorOr;

namespace PharmaPOS.Domain.SupplierAggregate;

public static class SupplierErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Supplier.NotFound", $"Supplier {id} does not exist.");

	public static readonly Error EmailNotUnique =
		Error.Conflict("Supplier.EmailNotUnique", "Email Address must be unique.");
}
