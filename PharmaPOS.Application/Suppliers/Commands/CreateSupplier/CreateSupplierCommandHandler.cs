using ErrorOr;
using Mapster;
using MediatR;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandHandler(
	ISupplierRepository supplierRepo,
	IUnitOfWork unitOfWork) : IRequestHandler<CreateSupplierCommand, ErrorOr<Created>>
{
	public async Task<ErrorOr<Created>> Handle(CreateSupplierCommand command, CancellationToken ct)
	{
		var request = command.Request;

		if (await supplierRepo.ExistsByEmailAsync(request.Email, null, ct))
			return SupplierApplicationErrors.EmailNotUnique;

		var supplierFields = request.Adapt<SupplierFields>();

		var supplierResult = Supplier.Create(supplierFields);
		if (supplierResult.IsError)
			return supplierResult.Errors;

		var supplier = supplierResult.Value;
		supplierRepo.Add(supplier);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Created;
	}
}
