using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels;

namespace ITSM.Repositories;

public interface IUserManagementRepository
{
    Task<bool> DeleteUserById(string userId);
    Task<List<User>> GetAllUsersToList();
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
    Task EditUser(string id, EditUserViewModel editmodel);
    Task<EditUserViewModel> CreateEditUserViewModel(string userId);
}