using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Role/[action]")]
public class RoleController(
    RoleManager<IdentityRole> roleManager
) : Controller
{
    private const string AdminRoleName = "Admin";

    [HttpGet]
    [Route("/Role")]
    [Route("/Role/Index")]
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        var list = new List<RoleListViewModel>();

        foreach (var role in roles)
        {
            var claims = await roleManager.GetClaimsAsync(role);
            var description = claims.FirstOrDefault(c => c.Type == "Description")?.Value ?? "";
            var createdDateClaim = claims.FirstOrDefault(c => c.Type == "CreatedDate")?.Value;
            var createdDate = "";
            if (!string.IsNullOrWhiteSpace(createdDateClaim) && DateTime.TryParse(createdDateClaim, out var parsedDate))
            {
                createdDate = parsedDate.ToString("dd/MM/yyyy");
            }

            list.Add(new RoleListViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = description,
                CreatedDate = createdDate,
                IsAdminRole = string.Equals(role.Name, AdminRoleName, StringComparison.OrdinalIgnoreCase)
            });
        }

        ViewData["page_title"] = "User Roles";
        return View(list);
    }

    [HttpGet]
    public IActionResult Add()
    {
        var model = new RoleFormViewModel
        {
            PermissionsGroup = GetAvailablePermissions()
        };

        ViewData["page_title"] = "New User Role";
        return View("RoleForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(RoleFormViewModel model)
    {
        model.PermissionsGroup = GetAvailablePermissions();

        if (!ModelState.IsValid)
        {
            ViewData["page_title"] = "New User Role";
            return View("RoleForm", model);
        }

        if (await roleManager.RoleExistsAsync(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), $"Role '{model.Name}' already exists.");
            ViewData["page_title"] = "New User Role";
            return View("RoleForm", model);
        }

        var role = new IdentityRole(model.Name);
        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewData["page_title"] = "New User Role";
            return View("RoleForm", model);
        }

        // Add Description & CreatedDate claims
        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            await roleManager.AddClaimAsync(role, new Claim("Description", model.Description));
        }
        await roleManager.AddClaimAsync(role, new Claim("CreatedDate", DateTime.UtcNow.ToString("o")));

        // Add Selected Permission claims
        if (model.SelectedPermissions.Count > 0)
        {
            foreach (var permission in model.SelectedPermissions)
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }

        TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "User role created successfully." });
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Role does not exist in database!" });
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(role.Name, AdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "You can not change Admin role setting." });
            return RedirectToAction(nameof(Index));
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var description = claims.FirstOrDefault(c => c.Type == "Description")?.Value ?? "";
        var selectedPermissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        var model = new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name ?? "",
            Description = description,
            SelectedPermissions = selectedPermissions,
            PermissionsGroup = GetAvailablePermissions()
        };

        ViewData["page_title"] = "Edit User Role";
        return View("RoleForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleFormViewModel model)
    {
        model.PermissionsGroup = GetAvailablePermissions();

        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return RedirectToAction(nameof(Index));
        }

        var role = await roleManager.FindByIdAsync(model.Id);
        if (role == null)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Role does not exist in database!" });
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(role.Name, AdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "You can not change Admin role setting." });
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewData["page_title"] = "Edit User Role";
            return View("RoleForm", model);
        }

        var existingWithSameName = await roleManager.FindByNameAsync(model.Name);
        if (existingWithSameName != null && existingWithSameName.Id != role.Id)
        {
            ModelState.AddModelError(nameof(model.Name), $"Role '{model.Name}' already exists.");
            ViewData["page_title"] = "Edit User Role";
            return View("RoleForm", model);
        }

        role.Name = model.Name;
        var updateResult = await roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewData["page_title"] = "Edit User Role";
            return View("RoleForm", model);
        }

        // Sync Description Claim
        var claims = await roleManager.GetClaimsAsync(role);
        var descClaim = claims.FirstOrDefault(c => c.Type == "Description");
        if (descClaim != null)
        {
            await roleManager.RemoveClaimAsync(role, descClaim);
        }
        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            await roleManager.AddClaimAsync(role, new Claim("Description", model.Description));
        }

        // Sync Permission Claims
        var existingPermClaims = claims.Where(c => c.Type == "Permission").ToList();
        foreach (var claim in existingPermClaims)
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }

        if (model.SelectedPermissions.Count > 0)
        {
            foreach (var perm in model.SelectedPermissions)
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", perm));
            }
        }

        TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "User role updated successfully." });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Role ID is missing!" });
            return RedirectToAction(nameof(Index));
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Role does not exist in database!" });
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(role.Name, AdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Admin role cannot be deleted!" });
            return RedirectToAction(nameof(Index));
        }

        var result = await roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "User Role deleted successfully." });
        }
        else
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting role: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return RedirectToAction(nameof(Index));
    }

    private static Dictionary<string, List<RolePermissionItem>> GetAvailablePermissions()
    {
        return new Dictionary<string, List<RolePermissionItem>>
        {
            ["Dashboard"] =
            [
                new() { Key = "dashboard", Name = "Dashboard" }
            ],
            ["Customer"] =
            [
                new() { Key = "customers", Name = "Customers List" },
                new() { Key = "customer/add", Name = "Customer Add" },
                new() { Key = "customer/edit", Name = "Customer Edit" },
                new() { Key = "customer/delete", Name = "Customer Delete" },
                new() { Key = "customer/view", Name = "Customer View" }
            ],
            ["POS/Bill"] =
            [
                new() { Key = "medicine/billing", Name = "POS/Bill List" },
                new() { Key = "medicine/billing/add", Name = "Bill Add" },
                new() { Key = "medicine/billing/edit", Name = "Bill Edit" },
                new() { Key = "medicine/billing/delete", Name = "Bill Delete" },
                new() { Key = "medicine/billing/view", Name = "Bill View" }
            ],
            ["Purchase"] =
            [
                new() { Key = "medicine/purchase", Name = "Purchase List" },
                new() { Key = "medicine/purchase/add", Name = "Purchase Add" },
                new() { Key = "medicine/purchase/edit", Name = "Purchase Edit" },
                new() { Key = "medicine/purchase/delete", Name = "Purchase Delete" },
                new() { Key = "medicine/purchase/view", Name = "Purchase View" }
            ],
            ["Stock adjustment"] =
            [
                new() { Key = "medicine/stock", Name = "Stock List" },
                new() { Key = "medicine/stock/delete", Name = "Stock Delete" }
            ],
            ["Medicine"] =
            [
                new() { Key = "medicines", Name = "Medicine List" },
                new() { Key = "medicine/add", Name = "Medicine Add" },
                new() { Key = "medicine/edit", Name = "Medicine Edit" },
                new() { Key = "medicine/delete", Name = "Medicine Delete" },
                new() { Key = "medicine/view", Name = "Medicine View" }
            ],
            ["Medicine Category"] =
            [
                new() { Key = "medicine/category", Name = "Medicine Category List" },
                new() { Key = "medicine/category/add", Name = "Medicine Category Add" },
                new() { Key = "medicine/category/edit", Name = "Medicine Category Edit" },
                new() { Key = "medicine/category/delete", Name = "Medicine Category Delete" }
            ],
            ["Reports"] =
            [
                new() { Key = "reports", Name = "All Reports" }
            ],
            ["User"] =
            [
                new() { Key = "users", Name = "User List" },
                new() { Key = "user/add", Name = "User Add" },
                new() { Key = "user/edit", Name = "User Edit" },
                new() { Key = "user/delete", Name = "User Delete" }
            ],
            ["System Info"] =
            [
                new() { Key = "info", Name = "System Info" }
            ],
            ["Payment Method"] =
            [
                new() { Key = "paymentmethod", Name = "Payment Methods" },
                new() { Key = "paymentmethod/add", Name = "Payment Method Add" },
                new() { Key = "paymentmethod/edit", Name = "Payment Method Edit" },
                new() { Key = "paymentmethod/delete", Name = "Payment Method Delete" }
            ],
            ["Media"] =
            [
                new() { Key = "get/media", Name = "Media on Modal" },
                new() { Key = "media/upload", Name = "Media Upload" },
                new() { Key = "media/delete", Name = "Media Delete" }
            ],
            ["Suppliers"] =
            [
                new() { Key = "suppliers", Name = "Suppliers List" },
                new() { Key = "supplier/add", Name = "Supplier Add" },
                new() { Key = "supplier/edit", Name = "Supplier Edit" },
                new() { Key = "supplier/delete", Name = "Supplier Delete" }
            ]
        };
    }
}
