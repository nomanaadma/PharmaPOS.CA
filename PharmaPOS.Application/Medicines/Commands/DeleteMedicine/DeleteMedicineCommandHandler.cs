using PharmaPOS.Domain.Common.Errors;
using ErrorOr;
using MediatR;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Medicines.Commands.DeleteMedicine;

public class DeleteMedicineCommandHandler(
	IMedicineRepository medicineRepo,
	IBatchRepository batchRepo,
	IUnitOfWork unitOfWork) : IRequestHandler<DeleteMedicineCommand, ErrorOr<Deleted>>
{
	public async Task<ErrorOr<Deleted>> Handle(DeleteMedicineCommand command, CancellationToken ct)
	{
		var medicine = await medicineRepo.GetByIdAsync(command.Id, ct);

		if (medicine == null)
			return CommonErrors<Medicine>.NotFound(command.Id);

		var hasBatches = await batchRepo.HasBatchesByMedicineAsync(command.Id, ct);
		if (hasBatches)
			return MedicineApplicationErrors.CannotDeleteWithBatches;

		medicineRepo.Remove(medicine);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Deleted;
	}
}

