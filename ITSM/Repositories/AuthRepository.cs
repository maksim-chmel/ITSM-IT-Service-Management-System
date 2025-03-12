using ITSM.Models;


namespace ITSM.Repositories;

public class AuthRepository:IAuthRepository
{
    public Task<User> FindByPhoneNumberAsync(string phoneNumber)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateUserAsync(User user, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddUserToRoleAsync(User user, string role)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckPasswordAsync(User user, string password)
    {
        throw new NotImplementedException();
    }

    public Task SignInAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task SignOutAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserExistAsync(string phoneNumber)
    {
        throw new NotImplementedException();
    }

    public Task<IList<string>> GetUserRolesAsync(User user)
    {
        throw new NotImplementedException();
    }
}