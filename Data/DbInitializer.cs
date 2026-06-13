using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace PharmaPOS_MVC.Data;

public static class DbInitializer
{
	
	public static async Task SeedAuthData(IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

		// 1. Define and Create Roles
		string[] roles = ["Admin", "Store Manager", "Employee"];
		
		foreach (var roleName in roles)
		{
			if (!await roleManager.RoleExistsAsync(roleName))
			{	
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}
		}

		// 2. Define Exact Permission Arrays
		string[] adminPermissions =
		[
			"dashboard", "customers", "customer/add", "customer/edit", "customer/delete", "customer/view",
			"medicine/billing", "medicine/billing/add", "medicine/billing/edit", "medicine/billing/delete", "medicine/billing/view",
			"medicine/purchase", "medicine/purchase/add", "medicine/purchase/edit", "medicine/purchase/delete", "medicine/purchase/view",
			"medicine/stock", "medicine/stock/delete", "medicines", "medicine/add", "medicine/edit", "medicine/delete", "medicine/view",
			"medicine/category", "medicine/category/add", "medicine/category/edit", "medicine/category/delete",
			"reportss", "users", "user/add", "user/edit", "user/delete", "info", "tax", "tax/add", "tax/edit", "tax/delete",
			"paymentmethod", "paymentmethod/add", "paymentmethod/edit", "paymentmethod/delete", "get/media", "media/upload", "media/delete"
		];

		string[] storeManagerPermissions =
		[
			"dashboard", "customers", "customer/add", "customer/edit", "customer/delete", "customer/view",
			"medicine/billing", "medicine/billing/add", "medicine/billing/edit", "medicine/billing/delete", "medicine/billing/view",
			"medicine/purchase", "medicine/purchase/add", "medicine/purchase/edit", "medicine/purchase/delete", "medicine/purchase/view",
			"medicine/stock", "medicine/stock/delete", "medicines", "medicine/add", "medicine/edit", "medicine/delete", "medicine/view",
			"medicine/category", "medicine/category/add", "medicine/category/edit", "medicine/category/delete",
			"tax", "tax/add", "tax/edit", "tax/delete", "paymentmethod", "paymentmethod/add", "paymentmethod/edit", "paymentmethod/delete",
			"get/media", "media/upload", "media/delete"
		];

		string[] employeePermissions =
		[
			"dashboard", "customers", "customer/add", "customer/edit", "customer/delete", "customer/view",
			"medicine/billing", "medicine/billing/add", "medicine/billing/edit", "medicine/billing/delete", "medicine/billing/view"
		];

		// 3. Assign Permissions as Claims to Roles
		await SyncRolePermissions(roleManager, "Admin", adminPermissions);
		await SyncRolePermissions(roleManager, "Store Manager", storeManagerPermissions);
		await SyncRolePermissions(roleManager, "Employee", employeePermissions);

		// 4. Create the Default Admin User
		const string adminEmail = "nomanaadma@gmail.com";
		var adminUser = await userManager.FindByEmailAsync(adminEmail);

		if (adminUser == null)
		{
			adminUser = new IdentityUser
			{
				UserName = adminEmail,
				Email = adminEmail,
				EmailConfirmed = true
			};

			var createResult = await userManager.CreateAsync(adminUser, "$upeEam#@!f0");
			if (createResult.Succeeded)
			{
				await userManager.AddToRoleAsync(adminUser, "Admin");
			}
		}
	}

	private static async Task SyncRolePermissions(RoleManager<IdentityRole> roleManager, string roleName, string[] permissions)
	{
		// 1. Fetch the role using the manager. This registers it cleanly in the current tracking scope.
		var role = await roleManager.FindByNameAsync(roleName);
		if (role == null) return;

		// 2. Safely extract the existing claims without detaching or re-attaching the role object.
		var existingClaims = await roleManager.GetClaimsAsync(role);

		foreach (var permission in permissions)
		{
			// 3. Check if the claim already exists to keep operation idempotent
			if (existingClaims.Any(c => c.Type == "Permission" && c.Value == permission)) continue;
			// 4. Use the native RoleManager API to inject the claim. 
			// This safely manages the state context internally without causing tracking collisions.
			var result = await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            
			if (!result.Succeeded)
			{
				throw new Exception($"Failed to add permission '{permission}' to role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
			}
		}
	}
}