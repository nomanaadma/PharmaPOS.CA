using System.Security.Claims;
using PharmaPOS_MVC.Authorization;

namespace PharmaPOS_MVC.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string permissionKey)
    {
	    return user.Identity is { IsAuthenticated: true } && user.HasClaim(c => c.Type == "Permission" && c.Value == permissionKey);
    }
    
}
