using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Models;
using System.Text.Json;

namespace PharmaPOS_MVC.Controllers;

// [Route("Customers/[action]")]
[Authorize]
[Route("Customers")]
public class CustomerController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
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
}
