using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.MedicineCategoryAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class CategoriesController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Category.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var categoriesQuery = await context.MedicineCategories
			.OrderBy(c => c.Id)
			.ToListAsync(ct);

		var categories = SettingMapper.MapToMedicineCategories(categoriesQuery);
		
		return View(categories);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Category.Add)]
	public IActionResult Add()
	{
		return View("CategoryForm", new MedicineCategoryViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Add)]
	public async Task<IActionResult> Add(MedicineCategoryViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View("CategoryForm", model);
		}

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name, ct))
		{
			ModelState.AddModelError(nameof(model.Name), "Category Name must be unique.");
			return View("CategoryForm", model);
		}

		try
		{
			var category = new MedicineCategory(model.Name);
			context.MedicineCategories.Add(category);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Category created successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error saving category: " + ex.Message);
			return View("CategoryForm", model);
		}
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Category.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (category == null)
		{
			NotifyError("Category not found.");
			return NotFound();
		}

		var model = SettingMapper.MapToMedicineCategory(category);

		return View("CategoryForm", model);
	}
	
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Edit)]
	public async Task<IActionResult> Edit(int id, MedicineCategoryViewModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		if (model.Id == 0)
		{
			NotifyError("Category ID is required for editing.");
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View("CategoryForm", model);
		}

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id, ct))
		{
			ModelState.AddModelError(nameof(model.Name), "Category Name must be unique.");
			return View("CategoryForm", model);
		}

		try
		{
			var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == model.Id, ct);
			if (category == null)
			{
				NotifyError("Category not found.");
				return NotFound();
			}

			category.Update(model.Name);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Category updated successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error updating category: " + ex.Message);
			return View("CategoryForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
			if (category == null)
			{
				NotifyError("Category not found.");
				return NotFound();
			}

			context.MedicineCategories.Remove(category);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Category deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting category: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}
}
