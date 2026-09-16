using ErrorOr;
using Mapster;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Application.Services;

public class CustomerService(
	ICustomerRepository customerRepo,
	IUnitOfWork unitOfWork,
	ILogger<CustomerService> logger) : IApplicationService
{
	/// <summary>
	/// Resolves an existing customer by ID, or creates a new walk-in customer from the billing form data.
	/// Returns ErrorOr&lt;int&gt; with the customer ID on success.
	/// </summary>
	public async Task<ErrorOr<int>> GetOrCreateCustomerAsync(CustomerRequest request, CancellationToken ct = default)
	{
		if (request.Id > 0)
			return request.Id.Value;

		if (string.IsNullOrWhiteSpace(request.FirstName))
			return BillErrors.CustomerNameRequired;
		
		return await SaveCustomerAsync(request, "Created walk-in customer {CustomerId} from billing form", ct);
	}

	public async Task<ErrorOr<Created>> CreateCustomerAsync(CustomerRequest request, CancellationToken ct = default)
	{
		if (request.Email != null && await customerRepo.ExistsByEmailAsync(request.Email, null, ct))
			return CustomerErrors.EmailNotUnique; 
		
		await SaveCustomerAsync(request, "Customer {CustomerId} created", ct);
		return Result.Created;
	}
	
	private async Task<ErrorOr<int>> SaveCustomerAsync(CustomerRequest request, string logMessage, CancellationToken ct)
	{
		var details = request.Adapt<CustomerDetails>();
		
		var customerResult = Customer.Create(details);
		if (customerResult.IsError)
			return customerResult.Errors;

		var customer = customerResult.Value;
		await customerRepo.AddAsync(customer, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation(logMessage, customer.Id);
		return customer.Id;
	}

	public async Task<ErrorOr<Updated>> UpdateCustomerAsync(int id, CustomerRequest request, CancellationToken ct = default)
	{
		var customer = await customerRepo.GetByIdAsync(id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);

		if (request.Email != null && await customerRepo.ExistsByEmailAsync(request.Email, id, ct))
			return CustomerErrors.EmailNotUnique;
		
		var details = request.Adapt<CustomerDetails>();
		
		var updateResult = customer.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		customerRepo.Update(customer);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Customer {CustomerId} updated", id);
		return Result.Updated;
	}

	public async Task<ErrorOr<Deleted>> DeleteCustomerAsync(int id, CancellationToken ct = default)
	{
		var customer = await customerRepo.GetByIdAsync(id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);

		customerRepo.Remove(customer);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Customer {CustomerId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<CustomerResponse>> GetCustomerAsync(int id, CancellationToken ct = default)
	{
		var customer = await customerRepo.GetByIdAsync(id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);
				
		var customerResponse =  customer.Adapt<CustomerResponse>();				

		return customerResponse;
	}

	public async Task<List<CustomerResponse>> GetAllCustomersAsync(CancellationToken ct = default)
	{
		var customers = await customerRepo.GetAllAsync(ct);
		var customerResponse = customers.Adapt<List<CustomerResponse>>();
		return customerResponse;
	}

	public async Task<List<CustomerSearchResult>> SearchCustomersAsync(string term, CancellationToken ct = default)
	{
		return await customerRepo.SearchCustomersAsync(term, 7, ct);
	}
}
