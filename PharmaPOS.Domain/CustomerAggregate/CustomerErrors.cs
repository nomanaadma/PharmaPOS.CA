using ErrorOr;

namespace PharmaPOS.Domain.CustomerAggregate;

public static class CustomerErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Customer.NotFound", $"Customer {id} does not exist.");

	public static Error FirstNameRequired =>
		Error.Validation("Customer.FirstNameRequired", "First Name is required.");

	public static Error LastNameRequired =>
		Error.Validation("Customer.LastNameRequired", "Last Name is required.");

	public static Error EmailNotUnique =>
		Error.Conflict("Customer.EmailNotUnique", "Email Address must be unique.");

	public static Error InvalidGender =>
		Error.Validation("Customer.InvalidGender", "Invalid gender value.");

	public static Error InvalidStatus =>
		Error.Validation("Customer.InvalidStatus", "Invalid status value.");
}
