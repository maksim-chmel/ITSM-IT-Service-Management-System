using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Manage;

public class ManageUserRolesViewModel
{
    [Required(ErrorMessage = "User ID is required.")]
    public string? UserId { get; set; }

    [Required(ErrorMessage = "User name is required.")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "At least one role must be selected.")]
    [MinLength(1, ErrorMessage = "At least one role must be selected.")]
    public List<RoleSelectionViewModel> Roles { get; set; } = new();
}