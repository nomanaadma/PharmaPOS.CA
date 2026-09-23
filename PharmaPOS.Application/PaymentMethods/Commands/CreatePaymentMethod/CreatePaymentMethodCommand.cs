using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.PaymentMethods.Commands.CreatePaymentMethod;

public record CreatePaymentMethodCommand(PaymentMethodRequest Request) : IRequest<ErrorOr<Created>>;
