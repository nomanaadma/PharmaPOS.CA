using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;
using PharmaPOS.Domain.SettingAggregate;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class SettingController(
	SettingService settingService
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Info.View)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var rawSetting = await settingService.GetRawSettingsAsync(ct); // await dbContext.Settings.FirstOrDefaultAsync(ct);
		var siteInfo = await settingService.GetSettingsAsync(ct);

		var model = SettingMapper.MapToSystemInfo(rawSetting, siteInfo);
		
		return View("Info", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Info.View)]
	public async Task<IActionResult> Index(SystemInfoViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
			NotifyError("Validation failed: " + errors);
			return View("Info", model);
		}

		var result = await settingService.UpdateSettingsAsync(model, ct);
		if (result.IsError)
		{
			NotifyError(result.Errors[0].Description);
			return View("Info", model);
		}

		NotifySuccess("System settings updated successfully.");

		return RedirectToAction(nameof(Index));
	}
}
