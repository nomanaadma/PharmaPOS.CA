using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.MVC.Data;

namespace PharmaPOS.MVC.Services;


public class PaymentMethodService(
	ApplicationDbContext context,
	ILogger<PaymentMethodService> logger)
{
	
	/// <summary>
	/// Returns active payment methods as SelectListItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<SelectListItem>> GetPaymentMethodsAsync(CancellationToken ct = default)
	{
		return await context.PaymentMethods
			.Where(pm => pm.Status == Status.Active)
			.Select(pm =>
				new SelectListItem
				{
					Value = pm.Id.ToString(),
					Text = pm.Name
				})
			.ToListAsync(ct);
	}
	
}