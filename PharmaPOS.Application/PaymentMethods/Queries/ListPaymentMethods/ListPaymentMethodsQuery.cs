using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.PaymentMethods.Queries.ListPaymentMethods;

public record ListPaymentMethodsQuery : IRequest<List<PaymentMethodResponse>>;
