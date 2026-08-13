using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class DashboardController : BaseController
{
	[Authorize(Policy = Permissions.Dashboard.View)]
	public IActionResult Index()
	{
		return View();
	}
	
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}

	[AllowAnonymous]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Status(int code)
	{
		return View(code);
	}
}