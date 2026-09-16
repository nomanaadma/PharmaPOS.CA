using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Razor.Pages.Billing;

[Authorize(Policy = Permissions.Billing.View)]
public class ViewModel(BillingService billingService, SettingService siteInfo) : BasePageModel
{
    public BillResponse Bill { get; set; } = null!;
    public SettingResponse SiteInfo { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var result = await billingService.GetBillAsync(id, ct);

        if (result.IsError)
            return HandleErrors(result.Errors);

        Bill =  result.Value;
        SiteInfo = await siteInfo.GetSettingsAsync(ct);
        
        return Page();
    }
}
