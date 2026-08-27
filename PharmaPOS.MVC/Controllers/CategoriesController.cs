using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Mappers;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class CategoriesController(
	CategoryService categoryService
) : BaseController
{
	private const string FormView = "CategoryForm";
	[HttpGet]
	[Authorize(Policy = Permissions.Category.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var categoriesQuery = await categoryService.GetAllCategoriesAsync(ct);
		var categories = SettingMapper.MapToMedicineCategories(categoriesQuery);
		
		return View(categories);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Category.Add)]
	public IActionResult Add()
	{
		return View(FormView, new MedicineCategoryViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Add)]
	public async Task<IActionResult> Add(MedicineCategoryViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}
		
		var request = model.Adapt<MedicineCategoryRequest>();

		var result = await categoryService.CreateCategoryAsync(request, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["Category.NameNotUnique"] = "Name" });

		NotifySuccess("Category created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Category.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await categoryService.GetCategoryAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var model = SettingMapper.MapToMedicineCategory(result.Value);
		return View(FormView, model);
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
			return View(FormView, model);
		}
		
		var request = model.Adapt<MedicineCategoryRequest>();

		var result = await categoryService.UpdateCategoryAsync(id, request, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["Category.NameNotUnique"] = "Name" });

		NotifySuccess("Category updated successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Category.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await categoryService.DeleteCategoryAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Category deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

}
