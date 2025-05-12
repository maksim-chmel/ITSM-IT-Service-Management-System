using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Repositories.UserProfile;
public class UserProfileService(UserManager<User> userManager, SignInManager<User> signInManager)
    : IUserProfileService
{
    public async Task<User?> GetUserAsync(ClaimsPrincipal userPrincipal)
    {
        return await userManager.GetUserAsync(userPrincipal);
    }

    public async Task<EditUserViewModel> GetUserProfileAsync(User user)
    {
        return  new EditUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    public async Task<IdentityResult> UpdateUserProfileAsync(User user, EditUserViewModel model)
    {
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword)
    {
        return await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task RefreshUserSignInAsync(User user)
    {
        await signInManager.RefreshSignInAsync(user);
    }
}
