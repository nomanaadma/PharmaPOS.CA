using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Razor.Pages.Billing;

public class GetBatchModel(BatchService batchService) : PageModel
{
    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var canAdd = User.HasClaim("Permission", Permissions.Billing.Add);
        var canEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
        
        if (!canAdd && !canEdit)
        {
            return Forbid();
        }
        
        var batches = await batchService.GetNonExpiredBatchesForMedicineAsync(id, ct);
        
        if (batches.Count == 0)
        {
            return new JsonResult(new { error = true, message = "No active batches found for this medicine." });
        }

        var resultHtml = "<option value=\"\">Select Batch</option>";
        foreach (var batch in batches)
        {
            resultHtml += $"<option value=\"{batch.Id}\">{batch.BatchName}</option>";
        }

        return new JsonResult(new { error = false, result = resultHtml });
    }
}
