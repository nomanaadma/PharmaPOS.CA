using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaPOS.Application.Authorization;

namespace PharmaPOS.Razor.Pages.Reports;

[Authorize(Policy = Permissions.Reports.View)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
