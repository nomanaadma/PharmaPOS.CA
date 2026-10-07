using ErrorOr;
using Mapster;
using MediatR;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.PaymentMethodAggregate;

namespace PharmaPOS.Application.PaymentMethods.Commands.CreatePaymentMethod;

public class CreatePaymentMethodCommandHandler(
	IPaymentMethodRepository paymentMethodRepo,
	IUnitOfWork unitOfWork) : IRequestHandler<CreatePaymentMethodCommand, ErrorOr<Created>>
{
	public async Task<ErrorOr<Created>> Handle(CreatePaymentMethodCommand command, CancellationToken ct)
	{
		var request = command.Request;

		if (await paymentMethodRepo.ExistsByNameAsync(request.Name, null, ct))
			return PaymentMethodApplicationErrors.NameNotUnique;

		var paymentMethodFields = request.Adapt<PaymentMethodFields>();

		var methodResult = PaymentMethod.Create(paymentMethodFields);
		if (methodResult.IsError)
			return methodResult.Errors;

		var method = methodResult.Value;
		paymentMethodRepo.Add(method);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Created;
	}
}
