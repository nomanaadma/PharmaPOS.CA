using System.ComponentModel.DataAnnotations;
using PharmaPOS_MVC.Authorization;

namespace PharmaPOS_MVC.Models;

public class RoleListViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string CreatedDate { get; set; } = "";
    public bool IsAdminRole { get; set; }
}

public class RolePermissionItem
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
}

public class RoleFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "User Role Name is required.")]
    [Display(Name = "User Role Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Description")]
    public string? Description { get; set; }

    public List<string> SelectedPermissions { get; set; } = [];

    public Dictionary<string, List<Permission>> PermissionsGroup { get; set; } = [];
}
