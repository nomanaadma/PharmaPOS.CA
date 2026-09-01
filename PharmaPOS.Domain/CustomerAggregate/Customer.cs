using ErrorOr;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Domain.CustomerAggregate;

public class Customer : AggregateRoot
{
	// EF Core parameterless constructor
	private Customer() { }

	private Customer(CustomerDetails details)
	{
		SetDetails(details);
	}
	
	public string FirstName { get; private set; } = null!;
	public string LastName { get; private set; } = null!;
	public string? Email { get; private set; }
	public string? Mobile { get; private set; }
	public Gender? Gender { get; private set; }
	public Status Status { get; private set; }
	public DateTime CreatedDate { get; private init; }

	public static ErrorOr<Customer> Create(CustomerDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Customer(details);
	}

	public ErrorOr<Updated> Update(CustomerDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}

	private void SetDetails(CustomerDetails details)
	{
		FirstName = details.FirstName;
		LastName = details.LastName;
		Email = details.Email;
		Mobile = details.Mobile;
		Gender = details.Gender;
		Status = details.Status;
	}

	private static List<Error> ValidateDetails(CustomerDetails details)
	{
		var errors = new List<Error>();
		
		DomainValidator<Customer>.ValidateRequiredStrings(errors,
			(details.FirstName, nameof(details.FirstName)),
			(details.LastName, nameof(details.LastName))
		);
		
		// Standard DomainValidator approach
		DomainValidator<Customer>.ValidateEnums(errors,
			(details.Gender, nameof(details.Gender)),
			(details.Status, nameof(details.Status))
		);
		
		return errors;
	}
}
