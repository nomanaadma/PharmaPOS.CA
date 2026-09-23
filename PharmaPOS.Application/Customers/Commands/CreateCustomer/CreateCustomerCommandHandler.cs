using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler(
	ICustomerRepository customerRepo,
	IUnitOfWork unitOfWork,
	ILogger<CreateCustomerCommandHandler> logger) : IRequestHandler<CreateCustomerCommand, ErrorOr<Created>>
{
	public async Task<ErrorOr<Created>> Handle(CreateCustomerCommand command, CancellationToken ct)
	{
		var request = command.Request;

		if (request.Email != null && await customerRepo.ExistsByEmailAsync(request.Email, null, ct))
			return CustomerErrors.EmailNotUnique; 
		
		var customerFields = request.Adapt<CustomerFields>();
		
		var customerResult = Customer.Create(customerFields);
		if (customerResult.IsError)
			return customerResult.Errors;

		var customer = customerResult.Value;
		await customerRepo.AddAsync(customer, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Customer {CustomerId} created", customer.Id);
		return Result.Created;
	}
}
