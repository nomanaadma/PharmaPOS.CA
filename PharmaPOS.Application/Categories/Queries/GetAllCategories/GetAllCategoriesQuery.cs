using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<List<CategoryResponse>>;
