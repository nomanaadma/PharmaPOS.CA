using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Medicines.Commands.DeleteMedicine;

public record DeleteMedicineCommand(int Id) : IRequest<ErrorOr<Deleted>>;
