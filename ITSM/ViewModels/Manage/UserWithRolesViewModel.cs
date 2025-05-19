using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Manage;

public class UserWithRolesViewModel
{
    [Required]
    public string? Id { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "UserName length can't be more than 50.")]
    public string? UserName { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    public List<string> Roles { get; set; } = new();

    public List<string> AssignedCategories { get; set; } = new();

    public bool IsDeleted { get; set; } = false;
}