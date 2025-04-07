using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels;

public class EditUserViewModel
{
    [Display(Name = "User Name")]
    public string? UserName { get; set; }

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [Display(Name = "Confirm New Password")]
    public string? ConfirmPassword { get; set; }
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string? CurrentPassword { get; set; }
}