using ErrorOr;
using MediatR;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Shared.Enums;

namespace PharmaPOS.Application.Stock.Commands.DeleteStock;

public class DeleteStockCommandHandler(
	IBatchRepository batchRepo,
	IUnitOfWork unitOfWork) : IRequestHandler<DeleteStockCommand, ErrorOr<Deleted>>
{
	public async Task<ErrorOr<Deleted>> Handle(DeleteStockCommand command, CancellationToken ct)
	{
		var batch = await batchRepo.GetByIdAsync(command.Id, ct);

		if (batch == null)
			return StockApplicationErrors.NotFound;
		
		batch.UpdateStatus(Status.InActive);
		await unitOfWork.SaveChangesAsync(ct);
		
		return Result.Deleted;
	}
}
