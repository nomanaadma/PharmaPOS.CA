using PharmaPOS.Domain.Common.Entities;

namespace PharmaPOS.Domain.SupplierAggregate;

public class Supplier : EntityWithDate
{
	// EF Core parameterless constructor
	private Supplier() { }

	public Supplier(SupplierDetails details)
	{
		Update(details);
	}

	public string Name { get; private set; } = null!;
	public string Email { get; private set; } = null!;
	public string Phone { get; private set; } = null!;
	public string? Address { get; private set; }

	public void Update(SupplierDetails details)
	{
		if (string.IsNullOrWhiteSpace(details.Name))
			throw new ArgumentException("Supplier Name is required.", nameof(details.Name));
		if (string.IsNullOrWhiteSpace(details.Email))
			throw new ArgumentException("Email Address is required.", nameof(details.Email));
		if (string.IsNullOrWhiteSpace(details.Phone))
			throw new ArgumentException("Phone Number is required.", nameof(details.Phone));

		Name = details.Name;
		Email = details.Email;
		Phone = details.Phone;
		Address = details.Address;
	}
}
