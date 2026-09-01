using ErrorOr;
using PharmaPOS.Domain.Common;

namespace PharmaPOS.Domain.SettingAggregate;

public class Setting : AggregateRoot
{
	// EF Core parameterless constructor
	private Setting() { }

	private Setting(SettingDetails details)
	{
		SetDetails(details);
		Address = Address.Empty;
	}

	public static ErrorOr<Setting> Create(SettingDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Setting(details);
	}

	public string? Logo { get; private set; }
	public string? LogoIcon { get; private set; }
	public string? LgBackground { get; private set; }
	public string? Favicon { get; private set; }
	public string Name { get; private set; } = null!;
	public string LegalName { get; private set; } = null!;
	public string Mail { get; private set; } = null!;
	public string Phone { get; private set; } = null!;
	public Address Address { get; private set; } = null!;

	public ErrorOr<Updated> Update(SettingDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;
		
		SetDetails(details);
		return Result.Updated;
	}
	
	private static List<Error> ValidateDetails(SettingDetails details)
	{
		var errors = new List<Error>();
		
		// Standard DomainValidator approach
		DomainValidator<Setting>.ValidateRequiredStrings(errors,
			(details.Name, nameof(details.Name)),
			(details.LegalName, nameof(details.LegalName)),
			(details.Mail, nameof(details.Mail)),
			(details.Phone, nameof(details.Phone))
		);
		
		return errors;
	}
	
	private void SetDetails(SettingDetails details)
	{
		Name = details.Name;
		LegalName = details.LegalName;
		Mail = details.Mail;
		Phone = details.Phone;
	}

	public void UpdateImages(string? logo, string? logoIcon, string? lgBackground, string? favicon)
	{
		Logo = logo;
		LogoIcon = logoIcon;
		LgBackground = lgBackground;
		Favicon = favicon;
	}

	public void UpdateAddress(Address address)
	{
		Address = address;
	}
}
