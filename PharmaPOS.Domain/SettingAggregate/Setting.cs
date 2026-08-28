using ErrorOr;
using PharmaPOS.Domain.Common.Entities;

namespace PharmaPOS.Domain.SettingAggregate;

public class Setting : Entity, IAggregateRoot
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
		
		if (string.IsNullOrWhiteSpace(details.Name))
			errors.Add(SettingErrors.NameRequired);
		
		if (string.IsNullOrWhiteSpace(details.LegalName))
			errors.Add(SettingErrors.LegalNameRequired);
		
		if (string.IsNullOrWhiteSpace(details.Mail))
			errors.Add(SettingErrors.MailRequired);
		
		if (string.IsNullOrWhiteSpace(details.Phone))
			errors.Add(SettingErrors.PhoneRequired);
		
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
