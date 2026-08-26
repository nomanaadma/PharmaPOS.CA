using System.Threading;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Authorization;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Controllers;

[Authorize(Roles = Roles.Admin)]
public class RolesController(
    RoleManager<IdentityRole> roleManager
) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync(ct);
        var list = new List<RoleListViewModel>();

        foreach (var role in roles)
        {
            var claims = await roleManager.GetClaimsAsync(role);
            var description = claims.GetClaim("Description");

            var createdDateClaim = claims.GetClaim("CreatedDate");
            
            var createdDate = "";
            if (!string.IsNullOrWhiteSpace(createdDateClaim) && DateTime.TryParse(createdDateClaim, out var parsedDate))
            {
                createdDate = parsedDate.Format();
            }

            list.Add(new RoleListViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = description,
                CreatedDate = createdDate,
            });
            
        }
        
        return View(list);
    }

    [HttpGet]
    public IActionResult Add()
    {
        var model = new RoleFormViewModel
        {
            PermissionsGroup = Permissions.GetPermissionsTemplate()
        };

        return View("RoleForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(RoleFormViewModel model, CancellationToken ct = default)
    {
        model.PermissionsGroup = Permissions.GetPermissionsTemplate();
        

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

        NotifySuccess("User role created successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            NotifyWarning("Role does not exist in database!");
            return NotFound();
        }

        if (role.Name == Roles.Admin)
        {
            NotifyWarning("You can not change Admin role setting.");
            return BadRequest();
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var description = claims.GetClaim("Description");
        var selectedPermissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        var model = RoleMapper.MapToRoleForm(role, description, selectedPermissions);

        return View("RoleForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RoleFormViewModel model, CancellationToken ct = default)
    {
        if (id != model.Id)
        {
            NotifyError("Bad Request: ID mismatch.");
            return BadRequest();
        }
        
        model.PermissionsGroup = Permissions.GetPermissionsTemplate();

        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return RedirectToAction(nameof(Index));
        }

        var role = await roleManager.FindByIdAsync(model.Id);
        if (role == null)
        {
            NotifyWarning("Role does not exist in database!");
            return NotFound();
        }

        if (role.Name == Roles.Admin)
        {
            NotifyWarning("You can not change Admin role setting.");
            return BadRequest();
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

        NotifySuccess("User role updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            NotifyError("Role ID is missing!");
            return NotFound();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            NotifyWarning("Role does not exist in database!");
            return NotFound();
        }

	    if (role.Name == Roles.Admin)   
        {
            NotifyError("Admin role cannot be deleted!");
            return BadRequest();
        }

        var result = await roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            NotifySuccess("User Role deleted successfully.");
        }
        else
        {
            NotifyError("Error deleting role: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return RedirectToAction(nameof(Index));
    }
    
}
