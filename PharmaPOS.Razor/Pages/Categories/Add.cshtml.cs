using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Pages.Categories;

[Authorize(Policy = Permissions.Category.Add)]
public class AddModel(CategoryService categoryService) : BasePageModel
{
    [BindProperty]
    public CategoryViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = Input.Adapt<CategoryRequest>();

        var result = await categoryService.CreateCategoryAsync(request, ct);
        if (result.IsError)
            return HandleErrors(result.Errors, new() { ["Category.NameNotUnique"] = "Input.Name" });

        NotifySuccess("Category created successfully.");
        return RedirectToPage("/Categories/Index");
    }
}
