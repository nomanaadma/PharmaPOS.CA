using ErrorOr;
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
		var newCustomer = new Customer(customerDetails);

		context.Customers.Add(newCustomer);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Created walk-in customer {CustomerId} from billing form", newCustomer.Id);
		return newCustomer.Id;
	}
}
