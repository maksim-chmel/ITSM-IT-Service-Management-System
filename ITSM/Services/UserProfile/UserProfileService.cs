using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Services.UserProfile;
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

    public async Task<OperationResult> UpdateUserProfileAsync(User user, EditUserViewModel model)
    {
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded) return OperationResult.Success("Profile updated successfully.");
        
        return OperationResult.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<OperationResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword)
    {
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded) return OperationResult.Success("Password changed successfully.");
        
        return OperationResult.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task RefreshUserSignInAsync(User user)
    {
        await signInManager.RefreshSignInAsync(user);
    }
}
