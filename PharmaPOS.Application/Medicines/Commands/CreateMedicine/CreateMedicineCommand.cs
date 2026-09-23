using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Medicines.Commands.CreateMedicine;

public record CreateMedicineCommand(MedicineRequest Request) : IRequest<ErrorOr<Created>>;
