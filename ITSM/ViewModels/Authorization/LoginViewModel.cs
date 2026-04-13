using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Authorization;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter an email address.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please enter a password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
