namespace ITSM.ViewModels;

public class ManageUserRolesViewModel
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public List<RoleSelectionViewModel> Roles { get; set; } = new();
}