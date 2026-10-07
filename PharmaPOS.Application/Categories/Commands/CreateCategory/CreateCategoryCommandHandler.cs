using ErrorOr;
using MediatR;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.CategoryAggregate;

namespace PharmaPOS.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
	ICategoryRepository categoryRepo,
	IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, ErrorOr<Created>>
{
	public async Task<ErrorOr<Created>> Handle(CreateCategoryCommand command, CancellationToken ct)
	{
		var request = command.Request;

		if (await categoryRepo.ExistsByNameAsync(request.Name, null, ct))
			return CategoryApplicationErrors.NameNotUnique;

		var categoryResult = Category.Create(request.Name);
		if (categoryResult.IsError)
			return categoryResult.Errors;

		var category = categoryResult.Value;
		categoryRepo.Add(category);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Created;
	}
}
