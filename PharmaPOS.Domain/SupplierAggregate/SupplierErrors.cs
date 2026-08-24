using ErrorOr;

namespace PharmaPOS.Domain.SupplierAggregate;

public static class SupplierErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Supplier.NotFound", $"Supplier {id} does not exist.");

	public static Error NameRequired =>
		Error.Validation("Supplier.NameRequired", "Supplier Name is required.");

	public static Error EmailRequired =>
		Error.Validation("Supplier.EmailRequired", "Email Address is required.");

	public static Error PhoneRequired =>
		Error.Validation("Supplier.PhoneRequired", "Phone Number is required.");

	public static Error EmailNotUnique =>
		Error.Conflict("Supplier.EmailNotUnique", "Email Address must be unique.");
}
