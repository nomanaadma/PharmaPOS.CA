using System.Security.Claims;
using PharmaPOS_MVC.Authorization;

namespace PharmaPOS_MVC.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string permissionKey)
    {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            return false;
        }

        return user.IsInRole(Roles.Admin) || user.HasClaim(c => c.Type == "Permission" && c.Value == permissionKey);
    }

    public static bool HasPermission(this ClaimsPrincipal user, Permission permission)
    {
        return user.HasPermission(permission.Key);
    }
}
