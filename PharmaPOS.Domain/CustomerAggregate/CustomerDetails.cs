using PharmaPOS.Shared.Enums;

namespace PharmaPOS.Domain.CustomerAggregate;

public record CustomerDetails(string FirstName, string LastName, string? Email, string? Mobile, Gender Gender, Status Status);
