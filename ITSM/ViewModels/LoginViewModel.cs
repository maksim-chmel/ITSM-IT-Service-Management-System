using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}
