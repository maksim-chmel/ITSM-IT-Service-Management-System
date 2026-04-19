using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.RoleManager;

public class UserRolesService(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager)
    : IUserRolesService
{
    public async Task<ManageUserRolesViewModel?> GetUserRolesViewModel(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await roleManager.Roles.ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);

        return new ManageUserRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Roles = roles.Select(r => new RoleSelectionViewModel
            {
                RoleName = r.Name,
                IsSelected = r.Name != null && userRoles.Contains(r.Name)
            }).ToList()
        };
    }


    public async Task<OperationResult> UpdateUserRolesAsync(string userId, IEnumerable<RoleSelectionViewModel> selectedRoles)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return OperationResult.Failure("User not found.");

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Where(r => r.IsSelected)
            .Select(r => r.RoleName).ToList();

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return OperationResult.Failure("Failed to remove current roles.");

        var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
        if (!addResult.Succeeded) return OperationResult.Failure("Failed to add new roles.");

        return OperationResult.Success("User roles updated successfully.");
    }
  

}