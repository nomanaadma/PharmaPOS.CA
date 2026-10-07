using ErrorOr;
using Mapster;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Application.Common.Services;

public class CustomerService(
	ICustomerRepository customerRepo,
	IUnitOfWork unitOfWork) : IApplicationService
{
	/// <summary>
	/// Resolves an existing customer by ID, or creates a new walk-in customer from the billing form data.
	/// Returns ErrorOr&lt;int&gt; with the customer ID on success.
	/// </summary>
	public async Task<ErrorOr<int>> GetOrCreateCustomerAsync(CustomerRequest request, CancellationToken ct = default)
	{
		if (request.Id > 0)
			return request.Id.Value;
			
		var existingCustomer = await customerRepo.FindByEmailOrPhoneAsync(request.Email, request.Mobile, ct);
		if (existingCustomer != null)
			return existingCustomer.Id;
		
		return await SaveCustomerAsync(request, ct);
	}
	
	public async Task<ErrorOr<int>> SaveCustomerAsync(CustomerRequest request, CancellationToken ct)
	{
		var details = request.Adapt<CustomerFields>();
		
		var customerResult = Customer.Create(details);
		if (customerResult.IsError)
			return customerResult.Errors;

		var customer = customerResult.Value;
		customerRepo.Add(customer);
		await unitOfWork.SaveChangesAsync(ct);

		return customer.Id;
	}
	
}