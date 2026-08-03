using System.ComponentModel.DataAnnotations;
using PharmaPOS_MVC.Authorization;

namespace PharmaPOS_MVC.Models;

public class RoleListViewModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string CreatedDate { get; init; } = "";
}

public class RoleFormViewModel
{
    public string? Id { get; init; }

    [Required(ErrorMessage = "User Role Name is required.")]
    [Display(Name = "User Role Name")]
    public string Name { get; init; } = "";

    [Display(Name = "Description")]
    public string? Description { get; init; }

    public List<string> SelectedPermissions { get; init; } = [];

    public Dictionary<string, Dictionary<string, string>> PermissionsGroup { get; set; } = [];
}
