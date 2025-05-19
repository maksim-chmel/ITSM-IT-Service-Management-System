using ITSM.Models;

namespace ITSM.Services.Authorisation;

public interface IAuthService
{
    Task RefreshSignInAsync(User user);
}