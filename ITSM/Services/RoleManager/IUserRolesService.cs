using ITSM.ViewModels.Manage;

namespace ITSM.Services.RoleManager;

public interface IUserRolesService
{
    Task<ManageUserRolesViewModel?> GetUserRolesViewModel(string userId);
    Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<RoleSelectionViewModel> selectedRoles);
}