using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Razor.Pages.Media;

[Authorize(Policy = Permissions.Media.Delete)]
public class DeleteModel(SettingService settingService) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string name, int id, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(name))
        {
            return new JsonResult(new { error = true, message = "File name is wrong or does not exist." });
        }

        var result = await settingService.DeleteMediaFileAsync(id, name, ct);
        return new JsonResult(result.IsError ? 
            new { error = true, message = result.Errors[0].Description } : 
            new { error = false, message = "File deleted successfully." });
    }
}
