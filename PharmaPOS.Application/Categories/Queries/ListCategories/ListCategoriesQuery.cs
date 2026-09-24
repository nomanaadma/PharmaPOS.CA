using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Categories.Queries.ListCategories;

public record ListCategoriesQuery : IRequest<List<CategoryResponse>>;
