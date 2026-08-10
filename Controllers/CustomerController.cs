using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Customers/[action]")]
public class CustomerController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/Customers")]
	[Authorize(Policy = Permissions.Customer.List)]
	public async Task<IActionResult> Index()
	{
		var customersQuery = await context.Customers
			.OrderByDescending(c => c.CreatedDate)
			.ToListAsync();

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
	public async Task<IActionResult> Add(CustomerViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("CustomerForm", model);
		}

		if (await context.Customers.AnyAsync(c => c.Email == model.Email))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("CustomerForm", model);
		}

		try
		{
			var customer = new Customer(model.FirstName, model.LastName, model.Email, model.Mobile, model.Gender!.Value, Status.Active);
			context.Customers.Add(customer);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Customer created successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving customer: " + ex.Message });
			return View("CustomerForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Customer.View)]
	public async Task<IActionResult> View(int id)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id);
		if (customer == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Customer does not exist in database!" });
			return RedirectToAction(nameof(Index));
		}

		var recentBills = await context.Bills
			.Where(b => b.CustomerId == customer.Id || b.Email == customer.Email)
			.OrderByDescending(b => b.BillDate)
			.Take(20)
			.ToListAsync();
		
		var model = new CustomerViewPageModel
		{
			Customer = CustomerMapper.MapToCustomer(customer),
			RecentBills = CustomerMapper.MapToCustomerBills(recentBills)
		};

		return View(model);
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Customer.Edit)]
	public async Task<IActionResult> Edit(int id)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id);
		if (customer == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Customer does not exist in database!" });
			return RedirectToAction(nameof(Index));
		}

		var model = CustomerMapper.MapToCustomer(customer);

		return View("CustomerForm", model);
	}

	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Edit)]
	public async Task<IActionResult> Edit(CustomerViewModel model)
	{
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Customer ID is required for editing." });
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			return View("CustomerForm", model);
		}

		if (await context.Customers.AnyAsync(c => c.Email == model.Email && c.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("CustomerForm", model);
		}

		try
		{
			var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == model.Id.Value);
			if (customer == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Customer not found." });
				return RedirectToAction(nameof(Index));
			}

			customer.Update(model.FirstName, model.LastName, model.Email, model.Mobile, model.Gender!.Value, model.Status);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Customer updated successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating customer: " + ex.Message });
			return View("CustomerForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Customer.Delete)]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id);
			if (customer == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Customer not found." });
				return RedirectToAction(nameof(Index));
			}

			context.Customers.Remove(customer);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Customer deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting customer: " + ex.Message });
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Route("/getcustomer")]
	[Authorize(Policy = Permissions.Medicine.List)]
	public async Task<IActionResult> SearchCustomer(string term)
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
			.ToListAsync();
		
		return Json(results);
	}
	
}
