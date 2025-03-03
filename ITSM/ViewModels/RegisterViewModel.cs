using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string? UserName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    public string? ConfirmPassword { get; set; }
}
