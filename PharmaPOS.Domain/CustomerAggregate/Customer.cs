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
	public string? Email { get; private set; }
	public string? Mobile { get; private set; }
	public Gender? Gender { get; private set; }
	public Status Status { get; private set; }

	public void Update(CustomerDetails details)
	{

		FirstName = details.FirstName;
		LastName = details.LastName;
		Email = details.Email;
		Mobile = details.Mobile;
		Gender = details.Gender;
		Status = details.Status;
	}
}
