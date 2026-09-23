using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Application.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierCommandHandler(
	ISupplierRepository supplierRepo,
	IUnitOfWork unitOfWork,
	ILogger<UpdateSupplierCommandHandler> logger) : IRequestHandler<UpdateSupplierCommand, ErrorOr<Updated>>
{
	public async Task<ErrorOr<Updated>> Handle(UpdateSupplierCommand command, CancellationToken ct)
	{
		var request = command.Request;

		var supplier = await supplierRepo.GetByIdAsync(command.Id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(command.Id);

		if (await supplierRepo.ExistsByEmailAsync(request.Email, command.Id, ct))
			return SupplierErrors.EmailNotUnique;

		var supplierFields = request.Adapt<SupplierFields>();

		var updateResult = supplier.Update(supplierFields);
		if (updateResult.IsError)
			return updateResult.Errors;

		supplierRepo.Update(supplier);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} updated", command.Id);
		return Result.Updated;
	}
}
