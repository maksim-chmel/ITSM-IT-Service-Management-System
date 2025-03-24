using ITSM.Models;
using Microsoft.AspNetCore.Identity;


namespace ITSM.Repositories;

public class AuthRepository(SignInManager<User> signInManager):IAuthRepository
{
    public async Task RefreshSignInAsync(User user)
    {
        await signInManager.SignOutAsync(); 
        await signInManager.SignInAsync(user, isPersistent: false); 
    }
}