using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Medicines.Commands.UpdateMedicine;

public class UpdateMedicineCommandHandler(
	IMedicineRepository medicineRepo,
	IUnitOfWork unitOfWork,
	ILogger<UpdateMedicineCommandHandler> logger) : IRequestHandler<UpdateMedicineCommand, ErrorOr<Updated>>
{
	public async Task<ErrorOr<Updated>> Handle(UpdateMedicineCommand command, CancellationToken ct)
	{
		var medicine = await medicineRepo.GetByIdAsync(command.Id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(command.Id);

		var medicineFields = command.Request.Adapt<MedicineFields>();

		var updateResult = medicine.Update(medicineFields);
		if (updateResult.IsError)
			return updateResult.Errors;

		medicineRepo.Update(medicine);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' updated", medicine.Id, medicine.Name);
		return Result.Updated;
	}
}
