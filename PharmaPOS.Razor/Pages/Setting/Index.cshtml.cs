using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;

namespace PharmaPOS.Razor.Pages.Setting;

[Authorize(Policy = Permissions.Info.View)]
public class IndexModel(SettingService settingService) : BasePageModel
{
    [BindProperty]
    public SystemInfoViewModel Input { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var rawSetting = await settingService.GetRawSettingsAsync(ct);
        var siteInfo = await settingService.GetSettingsAsync(ct);

        Input = SettingMapper.MapToSystemInfo(rawSetting, siteInfo);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            NotifyError("Validation failed: " + errors);
            return Page();
        }

        var request = Input.Adapt<SettingRequest>();

        var result = await settingService.UpdateSettingsAsync(request, ct);
        if (result.IsError)
        {
            NotifyError(result.Errors[0].Description);
            return Page();
        }

        NotifySuccess("System settings updated successfully.");

        return RedirectToPage();
    }
}
