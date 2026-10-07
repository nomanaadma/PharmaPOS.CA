using ErrorOr;
using Mapster;
using MediatR;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Medicines.Commands.CreateMedicine;

public class CreateMedicineCommandHandler(
	IMedicineRepository medicineRepo,
	IUnitOfWork unitOfWork) : IRequestHandler<CreateMedicineCommand, ErrorOr<Created>>
{
	public async Task<ErrorOr<Created>> Handle(CreateMedicineCommand command, CancellationToken ct)
	{
		var medicineFields = command.Request.Adapt<MedicineFields>();

		var medicineResult = Medicine.Create(medicineFields);
		if (medicineResult.IsError)
			return medicineResult.Errors;

		var medicine = medicineResult.Value;
		medicineRepo.Add(medicine);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Created;
	}
}
