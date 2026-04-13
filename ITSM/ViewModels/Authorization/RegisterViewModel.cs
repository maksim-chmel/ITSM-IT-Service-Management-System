using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Authorization;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter a username.")]
    [Display(Name = "Username")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Please enter an email address.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please enter a password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }
}
