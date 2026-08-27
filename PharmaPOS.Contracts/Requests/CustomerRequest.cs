using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Contracts.Requests;

public record CustomerRequest(
	int? Id,
	string FirstName,
	string LastName,
	string? Email,
	string? Mobile,
	Gender Gender,
	Status Status
);
