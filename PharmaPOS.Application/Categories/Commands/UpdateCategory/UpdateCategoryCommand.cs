using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(int Id, CategoryRequest Request) : IRequest<ErrorOr<Updated>>;
