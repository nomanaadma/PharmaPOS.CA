using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Models;
using System.Text.Json;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Customers/[action]")]
public class CustomerController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/Customers")]
	public async Task<IActionResult> Index()
	{
		var customers = await context.Customers
			.OrderByDescending(c => c.CreatedDate)
			.Select(c => new CustomerViewModel
			{
				Id = c.Id,
				FirstName = c.FirstName,
				LastName = c.LastName,
				Email = c.Email,
				Mobile = c.Mobile,
				Gender = c.Gender,
				Status = c.Status,
				CreatedDate = c.CreatedDate
			})
			.ToListAsync();

		return View(customers);
	}

	[HttpGet]
	public IActionResult Add()
	{
		return View("CustomerForm", new CustomerViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
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
	public async Task<IActionResult> Edit(int id)
	{
		var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == id);
		if (customer == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Customer does not exist in database!" });
			return RedirectToAction(nameof(Index));
		}

		var model = new CustomerViewModel
		{
			Id = customer.Id,
			FirstName = customer.FirstName,
			LastName = customer.LastName,
			Email = customer.Email,
			Mobile = customer.Mobile,
			Gender = customer.Gender,
			Status = customer.Status
		};

		return View("CustomerForm", model);
	}

	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
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
			var customer = await context.Customers.AsTracking().FirstOrDefaultAsync(c => c.Id == model.Id.Value);
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
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var customer = await context.Customers.AsTracking().FirstOrDefaultAsync(c => c.Id == id);
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
	public async Task<IActionResult> SearchCustomer(string term)
	{
		if (string.IsNullOrWhiteSpace(term))
		{
			return Json(Array.Empty<object>());
		}

		var results = await context.Customers
			.Where(c => EF.Functions.Like(c.FirstName, $"%{term}%") || 
			            EF.Functions.Like(c.LastName, $"%{term}%") || 
			            EF.Functions.Like(c.Mobile, $"%{term}%"))
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
