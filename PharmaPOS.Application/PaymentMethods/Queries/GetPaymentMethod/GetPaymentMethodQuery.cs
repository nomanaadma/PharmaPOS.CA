using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.PaymentMethods.Queries.GetPaymentMethod;

public record GetPaymentMethodQuery(int Id) : IRequest<ErrorOr<PaymentMethodResponse>>;
