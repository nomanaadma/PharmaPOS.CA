using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Purchases.Commands.DeletePurchase;

public record DeletePurchaseCommand(int Id) : IRequest<ErrorOr<Deleted>>;
