using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Stock.Commands.DeleteStock;

public record DeleteStockCommand(int Id) : IRequest<ErrorOr<Deleted>>;
