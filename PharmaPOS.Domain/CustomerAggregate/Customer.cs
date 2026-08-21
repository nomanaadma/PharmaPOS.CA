using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.Common.Guards;

namespace PharmaPOS.Domain.CustomerAggregate;

public class Customer : EntityWithDate
{
	// EF Core parameterless constructor
	private Customer() { }

	public Customer(CustomerDetails details)
	{
		Update(details);
	}
	
	public string FirstName { get; private set; } = null!;
	public string LastName { get; private set; } = null!;
	public string Email { get; private set; } = null!;
	public string Mobile { get; private set; } = null!;
	public Gender? Gender { get; private set; }
	public Status Status { get; private set; }

	public void Update(CustomerDetails details)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(details.FirstName);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.LastName);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Email);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Mobile);

		Guard.ThrowIfUndefined(details.Gender);
		Guard.ThrowIfUndefined(details.Status);

		FirstName = details.FirstName;
		LastName = details.LastName;
		Email = details.Email;
		Mobile = details.Mobile;
		Gender = details.Gender;
		Status = details.Status;
	}
}
