using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Categories/[action]")]
public class CategoryController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/Categories")]
	[Authorize(Policy = Permissions.Category.List)]
	public async Task<IActionResult> Index()
	{
		var categoriesQuery = await context.MedicineCategories
			.OrderBy(c => c.Id)
			.ToListAsync();

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
	public async Task<IActionResult> Add(MedicineCategoryViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("CategoryForm", model);
		}

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name))
		{
			ModelState.AddModelError(nameof(model.Name), "Category Name must be unique.");
			return View("CategoryForm", model);
		}

		try
		{
			var category = new MedicineCategory(model.Name);
			context.MedicineCategories.Add(category);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Category created successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving category: " + ex.Message });
			return View("CategoryForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Category.Edit)]
	public async Task<IActionResult> Edit(int id)
	{
		var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id);
		if (category == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category not found." });
			return RedirectToAction(nameof(Index));
		}

		var model = SettingMapper.MapToMedicineCategory(category);

		return View("CategoryForm", model);
	}
	
	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Edit)]
	public async Task<IActionResult> Edit(int id, MedicineCategoryViewModel model)
	{
		if (id != model.Id)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Bad Request: ID mismatch." });
			return RedirectToAction(nameof(Index));
		}
		
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category ID is required for editing." });
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			return View("CategoryForm", model);
		}

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Name), "Category Name must be unique.");
			return View("CategoryForm", model);
		}

		try
		{
			var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == model.Id.Value);
			if (category == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category not found." });
				return RedirectToAction(nameof(Index));
			}

			category.Update(model.Name);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Category updated successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating category: " + ex.Message });
			return View("CategoryForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Delete)]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id);
			if (category == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category not found." });
				return RedirectToAction(nameof(Index));
			}

			context.MedicineCategories.Remove(category);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Category deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting category: " + ex.Message });
		}

		return RedirectToAction(nameof(Index));
	}
}
