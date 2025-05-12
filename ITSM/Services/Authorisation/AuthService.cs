using ITSM.Models;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Repositories.Authorisation;

public class AuthService(SignInManager<User> signInManager):IAuthService
{
    public async Task RefreshSignInAsync(User user)
    {
        await signInManager.SignOutAsync(); 
        await signInManager.SignInAsync(user, isPersistent: false); 
    }
}