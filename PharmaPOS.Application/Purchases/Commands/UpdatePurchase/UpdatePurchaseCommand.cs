using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Purchases.Commands.UpdatePurchase;

public record UpdatePurchaseCommand(int Id, PurchaseRequest Request) : IRequest<ErrorOr<Updated>>;
