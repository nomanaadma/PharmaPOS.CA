using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Stock.Commands.UpdateStock;

public record UpdateStockCommand(int Id, int MedicineId, decimal Available) : IRequest<ErrorOr<Updated>>;
