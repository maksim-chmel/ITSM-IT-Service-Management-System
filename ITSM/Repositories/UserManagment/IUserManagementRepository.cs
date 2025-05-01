using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.UserManagment;

public interface IUserManagementRepository
{
    Task<User?> GetUserById(string id);
    Task<bool> DeleteUserById(string userId);
    Task<UserWithRolesViewModel[]> GetAllUsersToList();
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
    Task EditUser(string id, EditUserViewModel editmodel);
    Task<EditUserViewModel> CreateEditUserViewModel(string userId);
    Task<UserWithRolesViewModel[]> SearchUser(string? search);
    Task<bool> SoftDeleteUserById(string userId);

}