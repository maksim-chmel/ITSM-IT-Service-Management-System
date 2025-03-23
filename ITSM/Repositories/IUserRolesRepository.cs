using ITSM.ViewModels;

namespace ITSM.Repositories;

public interface IUserRolesRepository
{
    Task<ManageUserRolesViewModel?> GetUserRolesViewModel(string userId);
    Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<RoleSelectionViewModel> selectedRoles);
}