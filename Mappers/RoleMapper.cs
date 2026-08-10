using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class RoleMapper
{
	
    public static RoleFormViewModel MapToRoleForm(IdentityRole role, string description, List<string> selectedPermissions)
    {
        return new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name ?? "",
            Description = description,
            SelectedPermissions = selectedPermissions,
            PermissionsGroup = Permissions.GetPermissionsTemplate()
        };
    }
}
