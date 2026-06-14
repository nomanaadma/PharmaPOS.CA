namespace PharmaPOS_MVC.Domain.ValueObjects;

public class Address
{
	// Private parameterless constructor for EF Core
	private Address() { }

	public Address(string? line1, string? line2, string? city, string? country, string? postal)
	{
		Line1 = line1;
		Line2 = line2;
		City = city;
		Country = country;
		Postal = postal;
	}

	public string? Line1 { get; private set; }
	public string? Line2 { get; private set; }
	public string? City { get; private set; }
	public string? Country { get; private set; }
	public string? Postal { get; private set; }

	public static Address Empty => new Address(null, null, null, null, null);

	protected bool Equals(Address other)
	{
		return Line1 == other.Line1 &&
		       Line2 == other.Line2 &&
		       City == other.City &&
		       Country == other.Country &&
		       Postal == other.Postal;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((Address)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Line1, Line2, City, Country, Postal);
	}
}
