using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(int Id) : IRequest<ErrorOr<Deleted>>;
