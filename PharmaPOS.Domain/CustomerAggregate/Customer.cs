using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;

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
		if (string.IsNullOrWhiteSpace(details.FirstName))
			throw new ArgumentException("First Name is required.", nameof(details.FirstName));
		if (string.IsNullOrWhiteSpace(details.LastName))
			throw new ArgumentException("Last Name is required.", nameof(details.LastName));
		if (string.IsNullOrWhiteSpace(details.Email))
			throw new ArgumentException("Email Address is required.", nameof(details.Email));
		if (string.IsNullOrWhiteSpace(details.Mobile))
			throw new ArgumentException("Mobile Number is required.", nameof(details.Mobile));
		if (!Enum.IsDefined(details.Gender))
			throw new ArgumentException("Invalid gender value.", nameof(details.Gender));
		if (!Enum.IsDefined(details.Status))
			throw new ArgumentException("Invalid status value.", nameof(details.Status));

		FirstName = details.FirstName;
		LastName = details.LastName;
		Email = details.Email;
		Mobile = details.Mobile;
		Gender = details.Gender;
		Status = details.Status;
	}
}
