using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Razor.Pages.Billing;

public class GetBatchDataModel(BatchService batchService) : PageModel
{
    public async Task<IActionResult> OnPostAsync(int medicine, int batch, CancellationToken ct)
    {
        var canAdd = User.HasClaim("Permission", Permissions.Billing.Add);
        var canEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
        
        if (!canAdd && !canEdit)
        {
            return Forbid();
        }
        
        var mBatch = await batchService.GetSingleBatchAsync(medicine, batch, ct);

        if (mBatch == null)
        {
            return new JsonResult(new { error = true, message = "Batch not found." });
        }

        var result = new
        {
            expiry = mBatch.Expiry,
            available_quantity = mBatch.Pricing.Qty - mBatch.Sold,
            saleprice = mBatch.Pricing.SalePrice
        };

        return new JsonResult(new { error = false, result });
    }
}
