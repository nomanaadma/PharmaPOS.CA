using ErrorOr;

namespace PharmaPOS.Application.Customers;

public static class CustomerApplicationErrors
{
	public static readonly Error EmailNotUnique =
		Error.Conflict("Customer.EmailNotUnique", "Email Address must be unique.");
	
}
