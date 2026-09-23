using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.PaymentMethods.Queries.GetAllPaymentMethods;

public class GetAllPaymentMethodsQueryHandler(
	IPaymentMethodRepository paymentMethodRepo) : IRequestHandler<GetAllPaymentMethodsQuery, List<PaymentMethodResponse>>
{
	public async Task<List<PaymentMethodResponse>> Handle(GetAllPaymentMethodsQuery query, CancellationToken ct)
	{
		var methods = await paymentMethodRepo.GetAllAsync(ct);
		var methodsResponse = methods.Adapt<List<PaymentMethodResponse>>();
		return methodsResponse;
	}
}
