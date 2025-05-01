using ITSM.Models;
using ITSM.Repositories.Authorisation;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.RoleManager;

public class UserRolesRepository(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager)
    : IUserRolesRepository
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


    public async Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<RoleSelectionViewModel> selectedRoles)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Where(r => r.IsSelected)
            .Select(r => r.RoleName).ToList();

        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRolesAsync(user, rolesToAdd);
       

        return true;
    }
  

}