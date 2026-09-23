using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Medicines.Queries.GetMedicine;

public record GetMedicineQuery(int Id) : IRequest<ErrorOr<MedicineResponse>>;
