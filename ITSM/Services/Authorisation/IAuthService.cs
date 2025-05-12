using ITSM.Models;

namespace ITSM.Repositories.Authorisation;

public interface IAuthService
{
    Task RefreshSignInAsync(User user);
}