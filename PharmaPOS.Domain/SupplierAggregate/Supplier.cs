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
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Name);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Email);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Phone);

		Name = details.Name;
		Email = details.Email;
		Phone = details.Phone;
		Address = details.Address;
	}
}
