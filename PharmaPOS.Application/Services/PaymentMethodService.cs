using ErrorOr;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Data;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.PaymentMethodAggregate;

namespace PharmaPOS.Application.Services;

public class PaymentMethodService(
	ApplicationDbContext context,
	ILogger<PaymentMethodService> logger) : IApplicationService
{
	public async Task<ErrorOr<int>> CreatePaymentMethodAsync(PaymentMethodRequest request, CancellationToken ct = default)
	{
		if (await context.PaymentMethods.AnyAsync(p => p.Name == request.Name, ct))
			return PaymentMethodErrors.NameNotUnique;

		var details = request.Adapt<PaymentMethodDetails>();

		var methodResult = PaymentMethod.Create(details);
		if (methodResult.IsError)
			return methodResult.Errors;

		var method = methodResult.Value;
		context.PaymentMethods.Add(method);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Payment method {MethodId} '{Name}' created", method.Id, method.Name);
		return method.Id;
	}

	public async Task<ErrorOr<int>> UpdatePaymentMethodAsync(int id, PaymentMethodRequest request, CancellationToken ct = default)
	{
		var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id, ct);
		if (method == null)
			return PaymentMethodErrors.NotFound(id);

		if (await context.PaymentMethods.AnyAsync(p => p.Name == request.Name && p.Id != id, ct))
			return PaymentMethodErrors.NameNotUnique;

		var details = request.Adapt<PaymentMethodDetails>();

		var updateResult = method.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		await context.SaveChangesAsync(ct);

		logger.LogInformation("Payment method {MethodId} updated", id);
		return method.Id;
	}

	public async Task<ErrorOr<Deleted>> DeletePaymentMethodAsync(int id, CancellationToken ct = default)
	{
		var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id, ct);
		if (method == null)
			return PaymentMethodErrors.NotFound(id);

		context.PaymentMethods.Remove(method);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Payment method {MethodId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<PaymentMethod>> GetPaymentMethodAsync(int id, CancellationToken ct = default)
	{
		var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id, ct);
		if (method == null)
			return PaymentMethodErrors.NotFound(id);

		return method;
	}

	public async Task<List<PaymentMethod>> GetAllPaymentMethodsAsync(CancellationToken ct = default)
	{
		return await context.PaymentMethods
			.OrderBy(p => p.Id)
			.ToListAsync(ct);
	}

	/// <summary>
	/// Returns active payment methods as SelectListItems for dropdowns (used by BillingController).
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