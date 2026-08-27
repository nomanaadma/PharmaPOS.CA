using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Data;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.CustomerAggregate;

namespace PharmaPOS.Application.Services;

public class CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
{
	/// <summary>
	/// Resolves an existing customer by ID, or creates a new walk-in customer from the billing form data.
	/// Returns ErrorOr&lt;int&gt; with the customer ID on success.
	/// </summary>
	public async Task<ErrorOr<int>> GetOrCreateCustomerAsync(CustomerRequest request, CancellationToken ct = default)
	{
		if (request.Id is > 0)
			return request.Id.Value;

		if (string.IsNullOrWhiteSpace(request.FirstName))
			return BillErrors.CustomerNameRequired;
		
		return await SaveCustomerAsync(request, "Created walk-in customer {CustomerId} from billing form", ct);
		
	}
	
	
	public async Task<ErrorOr<int>> CreateCustomerAsync(CustomerRequest request, CancellationToken ct = default)
	{
		if (await context.Customers.AnyAsync(c => c.Email == request.Email, ct))
			return CustomerErrors.EmailNotUnique;
		
		return await SaveCustomerAsync(request, "Customer {CustomerId} created", ct);
		
	}
	
	private async Task<ErrorOr<int>> SaveCustomerAsync(CustomerRequest request, string logMessage, CancellationToken ct)
	{
		var details = request.Adapt<CustomerDetails>();
		
		var customerResult = Customer.Create(details);
		if (customerResult.IsError)
			return customerResult.Errors;

		var customer = customerResult.Value;
		context.Customers.Add(customer);
    
		await context.SaveChangesAsync(ct);

		logger.LogInformation(logMessage, customer.Id);
		return customer.Id;
	}

	public async Task<ErrorOr<int>> UpdateCustomerAsync(int id, CustomerRequest request, CancellationToken ct = default)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);

		if (await context.Customers.AnyAsync(c => c.Email == request.Email && c.Id != id, ct))
			return CustomerErrors.EmailNotUnique;
		
		var details = request.Adapt<CustomerDetails>();
		
		var updateResult = customer.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		await context.SaveChangesAsync(ct);

		logger.LogInformation("Customer {CustomerId} updated", id);
		return customer.Id;
	}

	public async Task<ErrorOr<Deleted>> DeleteCustomerAsync(int id, CancellationToken ct = default)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);

		context.Customers.Remove(customer);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Customer {CustomerId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<Customer>> GetCustomerAsync(int id, CancellationToken ct = default)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);

		return customer;
	}

	public async Task<List<Customer>> GetAllCustomersAsync(CancellationToken ct = default)
	{
		return await context.Customers
			.OrderByDescending(c => c.CreatedDate)
			.ToListAsync(ct);
	}

	public async Task<object> SearchCustomersAsync(string term, CancellationToken ct = default)
	{
		return await context.Customers
			.Where(c => EF.Functions.ILike(c.FirstName, $"%{term}%") || 
			            EF.Functions.ILike(c.LastName, $"%{term}%") || 
			            EF.Functions.ILike(c.Mobile ?? string.Empty, $"%{term}%"))
			.Take(7)
			.Select(c => new 
			{
				id = c.Id,
				label = c.FirstName + " " + c.LastName,
				email = c.Email,
				mobile = c.Mobile
			})
			.ToListAsync(ct);
	}
	
}
