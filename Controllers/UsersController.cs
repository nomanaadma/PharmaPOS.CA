using System.Threading;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Models;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Helpers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class UsersController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager
) : BaseController
{

    [HttpGet]
    [Authorize(Policy = Permissions.User.List)]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var users = await userManager.Users.ToListAsync(ct);
        var userList = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var roleName = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "No Role";

            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";

            var createdDate = claims.FirstOrDefault(c => c.Type == "CreatedDate")?.Value ?? "";
            
            var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            
            var status = (isLockedOut) ? Status.InActive : Status.Active;

            userList.Add(new UserListViewModel
            {
                Id = user.Id,
                FullName = firstName + " " + lastName, 
                Email = user.Email ?? "",
                Mobile = user.PhoneNumber ?? "",
                UserName = user.UserName ?? "",
                Role = roleName,
                CreatedDate = createdDate,
                Status = status
            });
        }
        
        userList = userList.OrderByDescending(u => u.CreatedDate).ToList();

        return View(userList);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.User.Add)]
    public async Task<IActionResult> Add(CancellationToken ct = default)
    {
        var model = new UserFormViewModel
        {
            Roles = await GetRolesSelectListAsync(ct)
        };

        return View("UserForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.User.Add)]
    public async Task<IActionResult> Add(UserFormViewModel model, CancellationToken ct = default)
    {
	    
        model.Roles = await GetRolesSelectListAsync(ct);
        
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return View("UserForm", model);
        }

        var existingByUsername = await userManager.FindByNameAsync(model.UserName);
        if (existingByUsername != null)
        {
            ModelState.AddModelError(nameof(model.UserName), $"User Name {model.UserName} already exists in database.");
            return View("UserForm", model);
        }

        var existingByEmail = await userManager.FindByEmailAsync(model.Email);
        if (existingByEmail != null)
        {
            ModelState.AddModelError(nameof(model.Email), $"Email {model.Email} already exists in database.");
            return View("UserForm", model);
        }

        var user = new IdentityUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.Mobile,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("UserForm", model);
        }

        // Add Profile Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.GivenName, model.FirstName),
            new(ClaimTypes.Surname, model.LastName),
            new("Gender", model.Gender),
            new("CreatedDate", model.CreatedDate.Format()),
        };
        await userManager.AddClaimsAsync(user, claims);

        // Assign Role
        if (!string.IsNullOrWhiteSpace(model.UserRole) && await roleManager.RoleExistsAsync(model.UserRole))
        {
            await userManager.AddToRoleAsync(user, model.UserRole);
        }

        NotifySuccess("Account created successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = Permissions.User.Edit)]
    public async Task<IActionResult> Edit(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            NotifyWarning("User does not exist in database!");
            return NotFound();
        }

        var claims = await userManager.GetClaimsAsync(user);
        var roleName = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";
        
        var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
        var status = (isLockedOut) ? Status.InActive : Status.Active;
        
        var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
        var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
        var gender = claims.FirstOrDefault(c => c.Type == "Gender")?.Value ?? "Male";

        var model = new UserFormViewModel
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Mobile = user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            Gender = gender,
            Status = status,
            UserRole = roleName,
            UserName = user.UserName ?? "",
            Roles = await GetRolesSelectListAsync(ct)
        };

        return View("UserForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.User.Edit)]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model, CancellationToken ct = default)
    {
        if (id != model.UserId)
        {
            NotifyError("Bad Request: ID mismatch.");
            return BadRequest();
        }
        
        model.Roles = await GetRolesSelectListAsync(ct);

        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            NotifyWarning("User does not exist in database!");
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("UserForm", model);
        }

        // Check username uniqueness
        var existingByUsername = await userManager.FindByNameAsync(model.UserName);
        if (existingByUsername != null && existingByUsername.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.UserName), $"User Name {model.UserName} already exists in database.");
            return View("UserForm", model);
        }

        // Check email uniqueness
        var existingByEmail = await userManager.FindByEmailAsync(model.Email);
        if (existingByEmail != null && existingByEmail.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), $"Email {model.Email} already exists in database.");
            return View("UserForm", model);
        }

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.Mobile;

        // Update Lockout status
        if (model.Status == Status.InActive)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("UserForm", model);
        }

        // Sync Claims
        var claims = await userManager.GetClaimsAsync(user);
        
        await ClaimHelper.UpdateOrCreateClaimAsync(ClaimTypes.GivenName, model.FirstName, claims, userManager, user);
        await ClaimHelper.UpdateOrCreateClaimAsync(ClaimTypes.Surname, model.LastName, claims, userManager, user);
        await ClaimHelper.UpdateOrCreateClaimAsync("Gender", model.Gender, claims, userManager, user);

        // Sync Role
        if (!string.IsNullOrWhiteSpace(model.UserRole) && await roleManager.RoleExistsAsync(model.UserRole))
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.UserRole))
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, model.UserRole);
            }
        }

        NotifySuccess("Account updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.User.Delete)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            NotifyError("User ID is missing!");
            return NotFound();
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            NotifyWarning("User does not exist in database!");
            return NotFound();
        }

        // Prevent deleting Primary Admin or Self
        var currentUserId = userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            NotifyError("You cannot delete your own account!");
            return BadRequest();
        }
        
        var roleName = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "No Role";

        if (roleName == Roles.Admin)
        {
            NotifyError("Administrator account cannot be deleted!");
            return BadRequest();
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            NotifySuccess("Account deleted successfully.");
        }
        else
        {
            NotifyError("Error deleting account: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetRolesSelectListAsync(CancellationToken ct = default)
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync(ct);
        return roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        });
    }
    
}
