using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Purchases.Commands.CreatePurchase;

public record CreatePurchaseCommand(PurchaseRequest Request) : IRequest<ErrorOr<Created>>;
