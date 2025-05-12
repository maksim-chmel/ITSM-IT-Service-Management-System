using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Repositories.UserProfile;

public interface IUserProfileService
{
    Task<User?> GetUserAsync(ClaimsPrincipal userPrincipal);
    Task<EditUserViewModel> GetUserProfileAsync(User user);
    Task<IdentityResult> UpdateUserProfileAsync(User user, EditUserViewModel model);
    Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword);
    Task RefreshUserSignInAsync(User user);
}
