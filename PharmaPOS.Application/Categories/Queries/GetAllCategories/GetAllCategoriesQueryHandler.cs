using Mapster;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler(
	ICategoryRepository categoryRepo) : IRequestHandler<GetAllCategoriesQuery, List<CategoryResponse>>
{
	public async Task<List<CategoryResponse>> Handle(GetAllCategoriesQuery query, CancellationToken ct)
	{
		var categories = await categoryRepo.GetAllAsync(ct);
		var categoriesResponse = categories.Adapt<List<CategoryResponse>>();
		return categoriesResponse;
	}
}
