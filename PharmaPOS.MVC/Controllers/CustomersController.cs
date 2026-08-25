using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;
using PharmaPOS.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class CustomersController(
	ApplicationDbContext context,
	CustomerService customerService
) : BaseController
{
	private const string FormView = "CustomerForm";
	[HttpGet]
	[Authorize(Policy = Permissions.Customer.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var customersQuery = await customerService.GetAllCustomersAsync(ct);
		var customers = CustomerMapper.MapToCustomers(customersQuery);
		return View(customers);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Customer.Add)]
	public IActionResult Add()
	{
		return View(FormView, new CustomerViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Add)]
	public async Task<IActionResult> Add(CustomerViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}

		var result = await customerService.CreateCustomerAsync(model, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["Customer.EmailNotUnique"] = "Email" });

		NotifySuccess("Customer created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Customer.View)]
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var result = await customerService.GetCustomerAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var customer = result.Value;
		var recentBills = await context.Bills
			.Where(b => b.CustomerId == customer.Id || b.Email == customer.Email)
			.OrderByDescending(b => b.Date)
			.Take(20)
			.ToListAsync(ct);
		
		var model = new CustomerViewPageModel
		{
			Customer = CustomerMapper.MapToCustomer(customer),
			RecentBills = CustomerMapper.MapToCustomerBills(recentBills)
		};

		return View(model);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Customer.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await customerService.GetCustomerAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var model = CustomerMapper.MapToCustomer(result.Value);
		return View(FormView, model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Edit)]
	public async Task<IActionResult> Edit(int id, CustomerViewModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		if (model.Id == 0)
		{
			NotifyError("Customer ID is required for editing.");
			return BadRequest();
		}
		
		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}

		var result = await customerService.UpdateCustomerAsync(id, model, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["Customer.EmailNotUnique"] = "Email" });

		NotifySuccess("Customer updated successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await customerService.DeleteCustomerAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Customer deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet("/getcustomer")]
	[Authorize(Policy = Permissions.Medicine.List)]
	public async Task<IActionResult> SearchCustomer(string term, CancellationToken ct = default)
	{
		var canAdd = User.HasClaim("Permission", Permissions.Billing.Add);
		var canEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
		
		if (!canAdd && !canEdit)
		{
			return Forbid();
		}
		
		if (string.IsNullOrWhiteSpace(term))
		{
			return Json(Array.Empty<object>());
		}

		var results = await context.Customers
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
		
		return Json(results);
	}

}
