using ITSM.Models;
namespace ITSM.Repositories;

public interface IAuthRepository
{
    Task<User> FindByPhoneNumberAsync(string phoneNumber); 
    Task<bool> CreateUserAsync(User user, string password);
    Task<bool> AddUserToRoleAsync(User user, string role);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task SignInAsync(User user);
    Task SignOutAsync();
    Task<bool> IsUserExistAsync(string phoneNumber);
    Task<IList<string>> GetUserRolesAsync(User user);
}