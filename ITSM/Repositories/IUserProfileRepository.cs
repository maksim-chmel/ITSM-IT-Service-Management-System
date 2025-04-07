using System.Security.Claims;
using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Repositories;

public interface IUserProfileRepository
{
    Task<User?> GetUserAsync(ClaimsPrincipal userPrincipal);
    Task<EditUserViewModel> GetUserProfileAsync(User user);
    Task<IdentityResult> UpdateUserProfileAsync(User user, EditUserViewModel model);
    Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword);
    Task RefreshUserSignInAsync(User user);
}
