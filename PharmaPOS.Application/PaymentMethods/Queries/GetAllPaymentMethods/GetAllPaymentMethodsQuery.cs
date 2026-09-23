using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.PaymentMethods.Queries.GetAllPaymentMethods;

public record GetAllPaymentMethodsQuery : IRequest<List<PaymentMethodResponse>>;
