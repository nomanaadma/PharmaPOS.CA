using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaPOS_MVC.Models;

public class UserListViewModel
{
    public string Id { get; set; } = null!;
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsPrimaryAdmin { get; set; }
}

public class UserFormViewModel
{
    public string? UserId { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last Name is required.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Mobile Number is required.")]
    [Phone(ErrorMessage = "Please enter a valid mobile number.")]
    [Display(Name = "Mobile Number")]
    public string Mobile { get; set; } = "";

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = "";

    [Display(Name = "Gender")]
    public string Gender { get; set; } = "Male";

    [Display(Name = "Status")]
    public int Status { get; set; } = 1; // 1 = Enabled, 0 = Disabled

    [Required(ErrorMessage = "User Role is required.")]
    [Display(Name = "User Role")]
    public string UserRole { get; set; } = "";

    [Required(ErrorMessage = "User Name is required.")]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    public IEnumerable<SelectListItem> Roles { get; set; } = [];
}
