using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.PaymentMethods.Commands.DeletePaymentMethod;

public record DeletePaymentMethodCommand(int Id) : IRequest<ErrorOr<Deleted>>;
