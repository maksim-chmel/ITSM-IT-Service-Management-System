using ITSM.Models;
namespace ITSM.Repositories;

public interface IAuthRepository
{
    Task RefreshSignInAsync(User user);
}