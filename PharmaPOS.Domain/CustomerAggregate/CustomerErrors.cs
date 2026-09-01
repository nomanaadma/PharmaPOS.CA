using ErrorOr;

namespace PharmaPOS.Domain.CustomerAggregate;

public static class CustomerErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Customer.NotFound", $"Customer {id} does not exist.");

	public static Error EmailNotUnique =>
		Error.Conflict("Customer.EmailNotUnique", "Email Address must be unique.");
	
}
