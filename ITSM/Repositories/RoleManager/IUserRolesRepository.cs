using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.RoleManager;

public interface IUserRolesRepository
{
    Task<ManageUserRolesViewModel?> GetUserRolesViewModel(string userId);
    Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<RoleSelectionViewModel> selectedRoles);
}