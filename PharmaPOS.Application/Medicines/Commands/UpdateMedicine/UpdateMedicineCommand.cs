using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Medicines.Commands.UpdateMedicine;

public record UpdateMedicineCommand(int Id, MedicineRequest Request) : IRequest<ErrorOr<Updated>>;
