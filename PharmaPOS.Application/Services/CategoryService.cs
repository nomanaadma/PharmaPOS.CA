using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MedicineCategoryAggregate;

namespace PharmaPOS.Application.Services;

public class CategoryService(
	IRepository<MedicineCategory> categoryRepo,
	ILogger<CategoryService> logger) : IApplicationService
{
	public async Task<ErrorOr<int>> CreateCategoryAsync(MedicineCategoryRequest request, CancellationToken ct = default)
	{
		if (await categoryRepo.AnyAsync(c => c.Name == request.Name, ct))
			return MedicineCategoryErrors.NameNotUnique;

		var categoryResult = MedicineCategory.Create(request.Name);
		if (categoryResult.IsError)
			return categoryResult.Errors;

		var category = categoryResult.Value;
		await categoryRepo.AddAsync(category, ct);
		await categoryRepo.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} '{Name}' created", category.Id, category.Name);
		return category.Id;
	}

	public async Task<ErrorOr<int>> UpdateCategoryAsync(int id, MedicineCategoryRequest request, CancellationToken ct = default)
	{
		var category = await categoryRepo.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
			return MedicineCategoryErrors.NotFound(id);

		if (await categoryRepo.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
			return MedicineCategoryErrors.NameNotUnique;

		var updateResult = category.Update(request.Name);
		if (updateResult.IsError)
			return updateResult.Errors;

		await categoryRepo.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} updated", id);
		return category.Id;
	}

	public async Task<ErrorOr<Deleted>> DeleteCategoryAsync(int id, CancellationToken ct = default)
	{
		var category = await categoryRepo.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
			return MedicineCategoryErrors.NotFound(id);

		categoryRepo.Remove(category);
		await categoryRepo.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<MedicineCategory>> GetCategoryAsync(int id, CancellationToken ct = default)
	{
		var category = await categoryRepo.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
			return MedicineCategoryErrors.NotFound(id);

		return category;
	}

	public async Task<List<MedicineCategory>> GetAllCategoriesAsync(CancellationToken ct = default)
	{
		return await categoryRepo.Query()
			.OrderBy(c => c.Id)
			.ToListAsync(ct);
	}
	
	/// <summary>
	/// Returns active medicine categories as LookupItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<LookupItem>> GetCategoriesAsync(CancellationToken ct = default)
	{
		return await categoryRepo.Query()
			.OrderBy(c => c.Name)
			.Select(c => new LookupItem(c.Id.ToString(), c.Name))
			.ToListAsync(ct);
	}
}
