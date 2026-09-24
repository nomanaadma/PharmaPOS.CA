using ErrorOr;

namespace PharmaPOS.Application.Suppliers;

public static class SupplierApplicationErrors
{
	public static readonly Error EmailNotUnique =
		Error.Conflict("Supplier.EmailNotUnique", "Email Address must be unique.");
}
