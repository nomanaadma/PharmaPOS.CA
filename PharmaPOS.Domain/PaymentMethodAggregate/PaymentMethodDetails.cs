using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Domain.PaymentMethodAggregate;

public record PaymentMethodDetails(string Name, Status Status);
