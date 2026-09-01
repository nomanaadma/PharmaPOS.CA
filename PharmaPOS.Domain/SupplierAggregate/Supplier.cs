using ErrorOr;
using PharmaPOS.Domain.Common;

namespace PharmaPOS.Domain.SupplierAggregate;

public class Supplier : AggregateRoot
{
	// EF Core parameterless constructor
	private Supplier() { }

	private Supplier(SupplierDetails details)
	{
		SetDetails(details);
	}

	public string Name { get; private set; } = null!;
	public string Email { get; private set; } = null!;
	public string Phone { get; private set; } = null!;
	public string? Address { get; private set; }
	public DateTime CreatedDate { get; private init; }


	public static ErrorOr<Supplier> Create(SupplierDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Supplier(details);
	}

	public ErrorOr<Updated> Update(SupplierDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}

	private void SetDetails(SupplierDetails details)
	{
		Name = details.Name;
		Email = details.Email;
		Phone = details.Phone;
		Address = details.Address;
	}

	private static List<Error> ValidateDetails(SupplierDetails details)
	{
		var errors = new List<Error>();

		DomainValidator<Supplier>.ValidateRequiredStrings(errors,
			(details.Name, nameof(details.Name)),
			(details.Email, nameof(details.Email)),
			(details.Phone, nameof(details.Phone))
		);

		return errors;
	}
}
