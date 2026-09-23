using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.PaymentMethods.Queries.GetPaymentMethodsLookup;

public class GetPaymentMethodsLookupQueryHandler(
	IPaymentMethodRepository paymentMethodRepo) : IRequestHandler<GetPaymentMethodsLookupQuery, IEnumerable<LookupItem>>
{
	public async Task<IEnumerable<LookupItem>> Handle(GetPaymentMethodsLookupQuery query, CancellationToken ct)
	{
		return await paymentMethodRepo.GetActiveLookupListAsync(ct);
	}
}
