using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Categories.Queries.GetCategoryLookup;

public class GetCategoryLookupQueryHandler(
	ICategoryRepository categoryRepo) : IRequestHandler<GetCategoryLookupQuery, IEnumerable<LookupItem>>
{
	public async Task<IEnumerable<LookupItem>> Handle(GetCategoryLookupQuery query, CancellationToken ct)
	{
		return await categoryRepo.GetLookupListAsync(ct);
	}
}
