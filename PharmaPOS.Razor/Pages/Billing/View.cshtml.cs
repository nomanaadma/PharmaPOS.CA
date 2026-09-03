using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Razor.Models;
using PharmaPOS.Razor.Mappers;
using Settings = PharmaPOS.Domain.SettingAggregate.Setting;

namespace PharmaPOS.Razor.Pages.Billing;

[Authorize(Policy = Permissions.Billing.View)]
public class ViewModel(BillingService billingService, SettingService siteInfo) : BasePageModel
{
    public BillingViewModel Bill { get; set; } = new();
    public Settings SiteInfo { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await billingService.GetBillAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        Bill = BillingMapper.MapToBillDetailsViewModel(result.Value);
        SiteInfo = await siteInfo.GetSettingsAsync(ct);
        
        return Page();
    }
}
