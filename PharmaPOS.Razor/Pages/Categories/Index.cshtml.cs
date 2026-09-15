using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Categories;

[Authorize(Policy = Permissions.Category.List)]
public class IndexModel(CategoryService categoryService) : BasePageModel
{
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var categoriesQuery = await categoryService.GetAllCategoriesAsync(ct);
        Categories = SettingMapper.MapToCategories(categoriesQuery);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var hasPermission = User.HasClaim("Permission", Permissions.Category.Delete);
        if (!hasPermission)
            return Forbid();

        var result = await categoryService.DeleteCategoryAsync(id, ct);
        if (result.IsError)
            return HandleErrors(result.Errors);

        NotifySuccess("Category deleted successfully.");
        return RedirectToPage();
    }
}
