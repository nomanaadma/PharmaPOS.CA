using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Categories.Queries.GetCategory;

public record GetCategoryQuery(int Id) : IRequest<ErrorOr<CategoryResponse>>;
