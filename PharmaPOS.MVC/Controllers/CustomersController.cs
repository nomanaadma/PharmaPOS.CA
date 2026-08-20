using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.Domain.Entities;
using PharmaPOS.Domain.Enums;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class CustomersController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Customer.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var customersQuery = await context.Customers
			.OrderByDescending(c => c.CreatedDate)
			.ToListAsync(ct);

		var customers = CustomerMapper.MapToCustomers(customersQuery);

		return View(customers);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Customer.Add)]
	public IActionResult Add()
	{
		return View("CustomerForm", new CustomerViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Add)]
	public async Task<IActionResult> Add(CustomerViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View("CustomerForm", model);
		}

		if (await context.Customers.AnyAsync(c => c.Email == model.Email, ct))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("CustomerForm", model);
		}

		try
		{
			var customer = new Customer(model.FirstName, model.LastName, model.Email, model.Mobile, model.Gender!.Value, Status.Active);
			context.Customers.Add(customer);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Customer created successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error saving customer: " + ex.Message);
			return View("CustomerForm", model);
		}
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Customer.View)]
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (customer == null)
		{
			NotifyWarning("Customer does not exist in database!");
			return NotFound();
		}

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
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (customer == null)
		{
			NotifyWarning("Customer does not exist in database!");
			return NotFound();
		}

		var model = CustomerMapper.MapToCustomer(customer);

		return View("CustomerForm", model);
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
			return View("CustomerForm", model);
		}

		if (await context.Customers.AnyAsync(c => c.Email == model.Email && c.Id != id, ct))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("CustomerForm", model);
		}

		try
		{
			var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
			if (customer == null)
			{
				NotifyError("Customer not found.");
				return NotFound();
			}

			customer.Update(model.FirstName, model.LastName, model.Email, model.Mobile, model.Gender!.Value, model.Status);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Customer updated successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error updating customer: " + ex.Message);
			return View("CustomerForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
			if (customer == null)
			{
				NotifyError("Customer not found.");
				return NotFound();
			}

			context.Customers.Remove(customer);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Customer deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting customer: " + ex.Message);
		}

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
			            EF.Functions.ILike(c.Mobile, $"%{term}%"))
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
