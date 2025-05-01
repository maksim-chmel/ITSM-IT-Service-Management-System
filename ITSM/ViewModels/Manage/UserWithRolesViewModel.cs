namespace ITSM.ViewModels.Manage;

public class UserWithRolesViewModel
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> AssignedCategories { get; set; } = new();
    public bool IsDeleted { get; set; } = false;
}