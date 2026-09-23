using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Purchases.Queries.GetPurchase;

public record GetPurchaseQuery(int Id) : IRequest<ErrorOr<PurchaseResponse>>;
