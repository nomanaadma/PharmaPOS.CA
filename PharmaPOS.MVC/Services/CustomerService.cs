using ErrorOr;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
{
	/// <summary>
	/// Resolves an existing customer by ID, or creates a new walk-in customer from the billing form data.
	/// Returns ErrorOr&lt;int&gt; with the customer ID on success.
	/// </summary>
	public async Task<ErrorOr<int>> GetOrCreateCustomerAsync(BillingFormModel model, CancellationToken ct = default)
	{
		if (model.CustomerId is > 0)
			return model.CustomerId.Value;

		if (string.IsNullOrWhiteSpace(model.Name))
			return BillErrors.CustomerNameRequired;

		var nameParts = model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var lastName = nameParts[^1];
		var firstName = nameParts.Length > 1
			? string.Join(" ", nameParts[..^1])
			: lastName;

		var customerDetails = new CustomerDetails(
			firstName,
			lastName,
			model.Email,
			model.Mobile,
			Gender.NotSpecified,
			Status.Active
		);
		
		return await SaveCustomerAsync(customerDetails, "Created walk-in customer {CustomerId} from billing form", ct);
		
	}
	
	
	public async Task<ErrorOr<int>> CreateCustomerAsync(CustomerViewModel model, CancellationToken ct = default)
	{
		if (await context.Customers.AnyAsync(c => c.Email == model.Email, ct))
			return CustomerErrors.EmailNotUnique;

		var details = new CustomerDetails(model.FirstName, model.LastName, model.Email, model.Mobile, model.Gender!.Value, Status.Active);
		
		return await SaveCustomerAsync(details, "Customer {CustomerId} created", ct);
		
	}
	
	private async Task<ErrorOr<int>> SaveCustomerAsync(CustomerDetails details, string logMessage, CancellationToken ct)
	{
		var customerResult = Customer.Create(details);
		if (customerResult.IsError)
			return customerResult.Errors;

		var customer = customerResult.Value;
		context.Customers.Add(customer);
    
		await context.SaveChangesAsync(ct);

		logger.LogInformation(logMessage, customer.Id);
		return customer.Id;
	}

	public async Task<ErrorOr<int>> UpdateCustomerAsync(int id, CustomerViewModel model, CancellationToken ct = default)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (customer == null)
			return CustomerErrors.NotFound(id);

		if (await context.Customers.AnyAsync(c => c.Email == model.Email && c.Id != id, ct))
			return CustomerErrors.EmailNotUnique;

		var details = new CustomerDetails(model.FirstName, model.LastName, model.Email, model.Mobile, model.Gender!.Value, model.Status);

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
	
}
