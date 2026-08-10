using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("Roles/[action]")]
public class RoleController(
    RoleManager<IdentityRole> roleManager
) : Controller
{

    [HttpGet("/Roles")]
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
            });
            
        }

        ViewData["Title"] = "User Roles";
        return View(list);
    }

    [HttpGet]
    public IActionResult Add()
    {
        var model = new RoleFormViewModel
        {
            PermissionsGroup = Permissions.GetPermissionsTemplate()
        };

        ViewData["Title"] = "New User Role";
        return View("RoleForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(RoleFormViewModel model)
    {
        model.PermissionsGroup = Permissions.GetPermissionsTemplate();
        ViewData["Title"] = "New User Role";
        

        if (!ModelState.IsValid)
        {
            return View("RoleForm", model);
        }

        if (await roleManager.RoleExistsAsync(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), $"Role '{model.Name}' already exists.");
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

        if (role.Name == Roles.Admin)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "You can not change Admin role setting." });
            return RedirectToAction(nameof(Index));
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var description = claims.FirstOrDefault(c => c.Type == "Description")?.Value ?? "";
        var selectedPermissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        var model = RoleMapper.MapToRoleForm(role, description, selectedPermissions);

        ViewData["Title"] = "Edit User Role";
        return View("RoleForm", model);
    }

    [HttpPost("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleFormViewModel model)
    {
        model.PermissionsGroup = Permissions.GetPermissionsTemplate();
        ViewData["Title"] = "Edit User Role";

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

        if (role.Name == Roles.Admin)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "You can not change Admin role setting." });
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View("RoleForm", model);
        }

        var existingWithSameName = await roleManager.FindByNameAsync(model.Name);
        if (existingWithSameName != null && existingWithSameName.Id != role.Id)
        {
            ModelState.AddModelError(nameof(model.Name), $"Role '{model.Name}' already exists.");
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

	    if (role.Name == Roles.Admin)   
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
    
}
