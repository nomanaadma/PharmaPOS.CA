using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Application.Customers.Commands.GetOrCreateCustomer;

public class GetOrCreateCustomerCommandHandler(
	ICustomerRepository customerRepo,
	IUnitOfWork unitOfWork,
	ILogger<GetOrCreateCustomerCommandHandler> logger) : IRequestHandler<GetOrCreateCustomerCommand, ErrorOr<int>>
{
	public async Task<ErrorOr<int>> Handle(GetOrCreateCustomerCommand command, CancellationToken ct)
	{
		var request = command.Request;

		if (request.Id > 0)
			return request.Id.Value;

		if (string.IsNullOrWhiteSpace(request.FirstName))
			return BillErrors.CustomerNameRequired;
		
		var customerFields = request.Adapt<CustomerFields>();
		
		var customerResult = Customer.Create(customerFields);
		if (customerResult.IsError)
			return customerResult.Errors;

		var customer = customerResult.Value;
		await customerRepo.AddAsync(customer, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Created walk-in customer {CustomerId} from billing form", customer.Id);
		return customer.Id;
	}
}
