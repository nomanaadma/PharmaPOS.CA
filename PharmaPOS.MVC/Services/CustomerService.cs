using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class CustomerService(ApplicationDbContext context, ILogger<BillingService> logger)
{
	/// <summary>
	/// Resolves an existing customer by ID, or creates a new walk-in customer from the billing form data.
	/// </summary>
	public async Task<int> GetOrCreateCustomerAsync(BillingFormModel model, CancellationToken ct = default)
	{
		if (model.CustomerId is > 0)
			return model.CustomerId.Value;

		if (string.IsNullOrWhiteSpace(model.Name))
			throw new InvalidOperationException("Customer name is required when no existing customer is selected.");

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
		var newCustomer = new Customer(customerDetails);

		context.Customers.Add(newCustomer);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Created walk-in customer {CustomerId} from billing form", newCustomer.Id);
		return newCustomer.Id;
	}
}
