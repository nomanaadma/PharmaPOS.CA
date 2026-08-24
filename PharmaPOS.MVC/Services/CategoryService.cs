using ErrorOr;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.MedicineCategoryAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class CategoryService(
	ApplicationDbContext context,
	ILogger<CategoryService> logger)
{
	public async Task<ErrorOr<int>> CreateCategoryAsync(MedicineCategoryViewModel model, CancellationToken ct = default)
	{
		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name, ct))
			return CategoryErrors.NameNotUnique;

		var categoryResult = MedicineCategory.Create(model.Name);
		if (categoryResult.IsError)
			return categoryResult.Errors;

		var category = categoryResult.Value;
		context.MedicineCategories.Add(category);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} '{Name}' created", category.Id, category.Name);
		return category.Id;
	}

	public async Task<ErrorOr<int>> UpdateCategoryAsync(int id, MedicineCategoryViewModel model, CancellationToken ct = default)
	{
		var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
			return CategoryErrors.NotFound(id);

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name && c.Id != id, ct))
			return CategoryErrors.NameNotUnique;

		var updateResult = category.Update(model.Name);
		if (updateResult.IsError)
			return updateResult.Errors;

		await context.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} updated", id);
		return category.Id;
	}

	public async Task<ErrorOr<Deleted>> DeleteCategoryAsync(int id, CancellationToken ct = default)
	{
		var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
			return CategoryErrors.NotFound(id);

		context.MedicineCategories.Remove(category);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Category {CategoryId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<MedicineCategory>> GetCategoryAsync(int id, CancellationToken ct = default)
	{
		var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
			return CategoryErrors.NotFound(id);

		return category;
	}

	public async Task<List<MedicineCategory>> GetAllCategoriesAsync(CancellationToken ct = default)
	{
		return await context.MedicineCategories
			.OrderBy(c => c.Id)
			.ToListAsync(ct);
	}
	
	/// <summary>
	/// Returns active medicine categories as SelectListItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<SelectListItem>> GetCategoriesAsync(CancellationToken ct = default)
	{
		return await context.MedicineCategories
			.OrderBy(c => c.Name)
			.Select(c => new SelectListItem
			{
				Value = c.Id.ToString(),
				Text = c.Name
			})
			.ToListAsync(ct);
	}
	
}
