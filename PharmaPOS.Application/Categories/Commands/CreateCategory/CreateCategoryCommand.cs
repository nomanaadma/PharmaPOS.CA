using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CategoryRequest Request) : IRequest<ErrorOr<Created>>;
