using PharmaPOS.Domain.Enums;

namespace PharmaPOS.Domain.Entities;

public class Customer : EntityWithDate
{
	// EF Core parameterless constructor
	private Customer() { }

	public Customer(string firstName, string lastName, string email, string mobile, Gender gender, Status status)
	{
		Update(firstName, lastName, email, mobile, gender, status);
	}
	
	public string FirstName { get; private set; } = null!;
	public string LastName { get; private set; } = null!;
	public string Email { get; private set; } = null!;
	public string Mobile { get; private set; } = null!;
	public Gender? Gender { get; private set; }
	public Status Status { get; private set; }

	public void Update(string firstName, string lastName, string email, string mobile, Gender gender, Status status)
	{
		if (string.IsNullOrWhiteSpace(firstName))
			throw new ArgumentException("First Name is required.", nameof(firstName));
		if (string.IsNullOrWhiteSpace(lastName))
			throw new ArgumentException("Last Name is required.", nameof(lastName));
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email Address is required.", nameof(email));
		if (string.IsNullOrWhiteSpace(mobile))
			throw new ArgumentException("Mobile Number is required.", nameof(mobile));
		if (!Enum.IsDefined(gender))
			throw new ArgumentException("Invalid gender value.", nameof(gender));
		if (!Enum.IsDefined(status))
			throw new ArgumentException("Invalid status value.", nameof(status));

		FirstName = firstName;
		LastName = lastName;
		Email = email;
		Mobile = mobile;
		Gender = gender;
		Status = status;
	}
}
