using System.Security.Claims;
using ITSM.Models;

namespace ITSM.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserById(string id);
    Task DeleteUserById(string id);
    Task<List<User>> GetAllUsersToList();
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
}