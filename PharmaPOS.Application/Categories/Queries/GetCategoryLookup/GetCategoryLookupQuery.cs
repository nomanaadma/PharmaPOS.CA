using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Categories.Queries.GetCategoryLookup;

public record GetCategoryLookupQuery : IRequest<IEnumerable<LookupItem>>;
