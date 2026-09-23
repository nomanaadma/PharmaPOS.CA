using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.PaymentMethods.Queries.GetPaymentMethodsLookup;

public record GetPaymentMethodsLookupQuery : IRequest<IEnumerable<LookupItem>>;
