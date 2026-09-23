using ErrorOr;
using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Medicines.Queries.GetMedicine;

public class GetMedicineQueryHandler(
	IMedicineRepository medicineRepo,
	ICategoryRepository categoryRepo) : IRequestHandler<GetMedicineQuery, ErrorOr<MedicineResponse>>
{
	public async Task<ErrorOr<MedicineResponse>> Handle(GetMedicineQuery query, CancellationToken ct)
	{
		var medicine = await medicineRepo.GetByIdAsync(query.Id, ct);
		if (medicine == null)
		{
			return MedicineErrors.NotFound(query.Id);
		}
		
		var categoryName = await categoryRepo.GetCategoryNameByIdAsync(medicine.CategoryId, ct);
		if (categoryName == null)
		{
			return MedicineErrors.CategoryNotFound(medicine.CategoryId);
		}
		
		var response = medicine.Adapt<MedicineResponse>() with
		{
			CategoryName = categoryName
		};

		return response;
	}
}
