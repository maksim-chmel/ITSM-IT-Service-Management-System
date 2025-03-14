using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels;

namespace ITSM.Repositories;

public interface IUserManagementRepository
{
    Task<User?> GetUserById(string id);
    Task DeleteUserById(string id);
    Task<List<User>> GetAllUsersToList();
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
    Task EditUser(string id, EditUserViewModel editmodel);
}