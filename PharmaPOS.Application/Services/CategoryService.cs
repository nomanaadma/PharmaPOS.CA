using ErrorOr;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.CategoryAggregate;

namespace PharmaPOS.Application.Services;

public class CategoryService(
	ICategoryRepository categoryRepo,
	IUnitOfWork unitOfWork,
	ILogger<CategoryService> logger) : IApplicationService
{
	public async Task<ErrorOr<Created>> CreateCategoryAsync(CategoryRequest request, CancellationToken ct = default)
	{
		if (await categoryRepo.ExistsByNameAsync(request.Name, null, ct))
			return CategoryErrors.NameNotUnique;

		var categoryResult = Category.Create(request.Name);
		if (categoryResult.IsError)
			return categoryResult.Errors;

		var category = categoryResult.Value;
		await categoryRepo.AddAsync(category, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} '{Name}' created", category.Id, category.Name);
		return Result.Created;
	}

	public async Task<ErrorOr<Updated>> UpdateCategoryAsync(int id, CategoryRequest request, CancellationToken ct = default)
	{
		var category = await categoryRepo.GetByIdAsync(id, ct);
		if (category == null)
			return CategoryErrors.NotFound(id);

		if (await categoryRepo.ExistsByNameAsync(request.Name, id, ct))
			return CategoryErrors.NameNotUnique;

		var updateResult = category.Update(request.Name);
		if (updateResult.IsError)
			return updateResult.Errors;

		categoryRepo.Update(category);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} updated", id);
		return Result.Updated;
	}

	public async Task<ErrorOr<Deleted>> DeleteCategoryAsync(int id, CancellationToken ct = default)
	{
		var category = await categoryRepo.GetByIdAsync(id, ct);
		if (category == null)
			return CategoryErrors.NotFound(id);

		categoryRepo.Remove(category);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<Category>> GetCategoryAsync(int id, CancellationToken ct = default)
	{
		var category = await categoryRepo.GetByIdAsync(id, ct);
		if (category == null)
			return CategoryErrors.NotFound(id);

		return category;
	}

	public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default)
	{
		return await categoryRepo.GetAllAsync(ct);
	}
	
	/// <summary>
	/// Returns active medicine categories as LookupItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<LookupItem>> GetCategoriesAsync(CancellationToken ct = default)
	{
		return await categoryRepo.GetLookupListAsync(ct);
	}
}
