using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Manage;

public class RoleSelectionViewModel
{
    [Required(ErrorMessage = "Role name is required")]
    public string? RoleName { get; set; }
    public bool IsSelected { get; set; }
}