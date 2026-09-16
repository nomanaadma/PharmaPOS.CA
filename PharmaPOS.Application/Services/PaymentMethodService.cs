using ErrorOr;
using Mapster;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.PaymentMethodAggregate;

namespace PharmaPOS.Application.Services;

public class PaymentMethodService(
	IPaymentMethodRepository paymentMethodRepo,
	IUnitOfWork unitOfWork,
	ILogger<PaymentMethodService> logger) : IApplicationService
{
	public async Task<ErrorOr<Created>> CreatePaymentMethodAsync(PaymentMethodRequest request, CancellationToken ct = default)
	{
		if (await paymentMethodRepo.ExistsByNameAsync(request.Name, null, ct))
			return PaymentMethodErrors.NameNotUnique;

		var details = request.Adapt<PaymentMethodDetails>();

		var methodResult = PaymentMethod.Create(details);
		if (methodResult.IsError)
			return methodResult.Errors;

		var method = methodResult.Value;
		await paymentMethodRepo.AddAsync(method, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Payment method {MethodId} '{Name}' created", method.Id, method.Name);
		return Result.Created;
	}

	public async Task<ErrorOr<Updated>> UpdatePaymentMethodAsync(int id, PaymentMethodRequest request, CancellationToken ct = default)
	{
		var method = await paymentMethodRepo.GetByIdAsync(id, ct);
		if (method == null)
			return PaymentMethodErrors.NotFound(id);

		if (await paymentMethodRepo.ExistsByNameAsync(request.Name, id, ct))
			return PaymentMethodErrors.NameNotUnique;

		var details = request.Adapt<PaymentMethodDetails>();

		var updateResult = method.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		paymentMethodRepo.Update(method);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Payment method {MethodId} updated", id);
		return Result.Updated;
	}

	public async Task<ErrorOr<Deleted>> DeletePaymentMethodAsync(int id, CancellationToken ct = default)
	{
		var method = await paymentMethodRepo.GetByIdAsync(id, ct);
		if (method == null)
			return PaymentMethodErrors.NotFound(id);

		paymentMethodRepo.Remove(method);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Payment method {MethodId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<PaymentMethodResponse>> GetPaymentMethodAsync(int id, CancellationToken ct = default)
	{
		var method = await paymentMethodRepo.GetByIdAsync(id, ct);
		if (method == null)
			return PaymentMethodErrors.NotFound(id);
		
		var methodResponse = method.Adapt<PaymentMethodResponse>();

		return methodResponse;

	}

	public async Task<List<PaymentMethodResponse>> GetAllPaymentMethodsAsync(CancellationToken ct = default)
	{
		var methods = await paymentMethodRepo.GetAllAsync(ct);
		var methodsResponse = methods.Adapt<List<PaymentMethodResponse>>(); 
		return methodsResponse;
	}

	/// <summary>
	/// Returns active payment methods as LookupItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<LookupItem>> GetPaymentMethodsAsync(CancellationToken ct = default)
	{
		return await paymentMethodRepo.GetActiveLookupListAsync(ct);
	}
}