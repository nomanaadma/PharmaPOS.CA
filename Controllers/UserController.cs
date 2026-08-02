using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("User/[action]")]
public class UserController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager
) : Controller
{
    private const string PrimaryAdminEmail = "nomanaadma@gmail.com";

    [HttpGet]
    [Route("/Users")]
    [Route("/User")]
    [Route("/User/Index")]
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users.ToListAsync();
        var userList = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);

            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";
            var roleName = roles.FirstOrDefault() ?? "No Role";

            var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            userList.Add(new UserListViewModel
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? "",
                Mobile = user.PhoneNumber ?? "",
                UserName = user.UserName ?? "",
                Role = roleName,
                IsActive = !isLockedOut,
                IsPrimaryAdmin = string.Equals(user.Email, PrimaryAdminEmail, StringComparison.OrdinalIgnoreCase)
            });
        }

        ViewData["page_title"] = "Users";
        return View(userList);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var model = new UserFormViewModel
        {
            Roles = await GetRolesSelectListAsync()
        };

        ViewData["page_title"] = "Add User";
        return View("UserForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(UserFormViewModel model)
    {
        model.Roles = await GetRolesSelectListAsync();

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["page_title"] = "Add User";
            return View("UserForm", model);
        }

        var existingByUsername = await userManager.FindByNameAsync(model.UserName);
        if (existingByUsername != null)
        {
            ModelState.AddModelError(nameof(model.UserName), $"User Name {model.UserName} already exists in database.");
            ViewData["page_title"] = "Add User";
            return View("UserForm", model);
        }

        var existingByEmail = await userManager.FindByEmailAsync(model.Email);
        if (existingByEmail != null)
        {
            ModelState.AddModelError(nameof(model.Email), $"Email {model.Email} already exists in database.");
            ViewData["page_title"] = "Add User";
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
            ViewData["page_title"] = "Add User";
            return View("UserForm", model);
        }

        // Add Profile Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.GivenName, model.FirstName),
            new(ClaimTypes.Surname, model.LastName),
            new("Gender", model.Gender ?? "Male")
        };
        await userManager.AddClaimsAsync(user, claims);

        // Assign Role
        if (!string.IsNullOrWhiteSpace(model.UserRole) && await roleManager.RoleExistsAsync(model.UserRole))
        {
            await userManager.AddToRoleAsync(user, model.UserRole);
        }

        TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Account created successfully." });
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "User does not exist in database!" });
            return RedirectToAction(nameof(Index));
        }

        var claims = await userManager.GetClaimsAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        var isLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

        var model = new UserFormViewModel
        {
            UserId = user.Id,
            FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "",
            LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "",
            Mobile = user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            Gender = claims.FirstOrDefault(c => c.Type == "Gender")?.Value ?? "Male",
            Status = isLockedOut ? 0 : 1,
            UserRole = roles.FirstOrDefault() ?? "",
            UserName = user.UserName ?? "",
            Roles = await GetRolesSelectListAsync()
        };

        ViewData["page_title"] = "Edit User";
        return View("UserForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormViewModel model)
    {
        model.Roles = await GetRolesSelectListAsync();

        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "User does not exist in database!" });
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewData["page_title"] = "Edit User";
            return View("UserForm", model);
        }

        // Check username uniqueness
        var existingByUsername = await userManager.FindByNameAsync(model.UserName);
        if (existingByUsername != null && existingByUsername.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.UserName), $"User Name {model.UserName} already exists in database.");
            ViewData["page_title"] = "Edit User";
            return View("UserForm", model);
        }

        // Check email uniqueness
        var existingByEmail = await userManager.FindByEmailAsync(model.Email);
        if (existingByEmail != null && existingByEmail.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), $"Email {model.Email} already exists in database.");
            ViewData["page_title"] = "Edit User";
            return View("UserForm", model);
        }

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.Mobile;

        // Update Lockout status
        if (model.Status == 0)
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
            ViewData["page_title"] = "Edit User";
            return View("UserForm", model);
        }

        // Sync Claims
        var claims = await userManager.GetClaimsAsync(user);
        var givenNameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
        if (givenNameClaim != null)
            await userManager.ReplaceClaimAsync(user, givenNameClaim, new Claim(ClaimTypes.GivenName, model.FirstName));
        else
            await userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, model.FirstName));

        var surnameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
        if (surnameClaim != null)
            await userManager.ReplaceClaimAsync(user, surnameClaim, new Claim(ClaimTypes.Surname, model.LastName));
        else
            await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, model.LastName));

        var genderClaim = claims.FirstOrDefault(c => c.Type == "Gender");
        if (genderClaim != null)
            await userManager.ReplaceClaimAsync(user, genderClaim, new Claim("Gender", model.Gender ?? "Male"));
        else
            await userManager.AddClaimAsync(user, new Claim("Gender", model.Gender ?? "Male"));

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

        TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Account updated successfully." });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "User ID is missing!" });
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "User does not exist in database!" });
            return RedirectToAction(nameof(Index));
        }

        // Prevent deleting Primary Admin or Self
        var currentUserId = userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "You cannot delete your own account!" });
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(user.Email, PrimaryAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Primary administrator account cannot be deleted!" });
            return RedirectToAction(nameof(Index));
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Account deleted successfully." });
        }
        else
        {
            TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting account: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetRolesSelectListAsync()
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        return roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        });
    }
}
