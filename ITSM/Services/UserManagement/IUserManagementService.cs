using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels.Manage;

namespace ITSM.Services.UserManagement;

public interface IUserManagementService
{
    Task<User?> GetUserById(string id);
   // Task<bool> DeleteUserById(string userId);
    Task<UserWithRolesViewModel[]> GetAllUsersToList();
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
    Task<OperationResult> EditUser(string id, EditUserViewModel editmodel);
    Task<EditUserViewModel> CreateEditUserViewModel(string userId);
    Task<UserWithRolesViewModel[]> SearchUser(string? search);
    Task<OperationResult> SoftDeleteUserById(string userId);

}