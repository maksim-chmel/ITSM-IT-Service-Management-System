using ITSM.Models;

namespace ITSM.Repositories.Authorisation;

public interface IAuthRepository
{
    Task RefreshSignInAsync(User user);
}