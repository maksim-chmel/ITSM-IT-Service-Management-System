using ITSM.Data;
using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.RoleManager;

public class UserRolesService(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    DBaseContext context)
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
            UserName = user.UserName ?? "Unknown",
            Roles = roles.Select(role => new RoleSelectionViewModel
            {
                RoleName = role.Name,
                IsSelected = userRoles.Contains(role.Name!)
            }).ToList()
        };
    }


    public async Task<OperationResult> UpdateUserRolesAsync(string userId, IEnumerable<RoleSelectionViewModel> selectedRoles)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return OperationResult.Failure("User not found.");

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Where(r => r.IsSelected && r.RoleName != null)
            .Select(r => r.RoleName!).ToList();

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return OperationResult.Failure("Failed to remove current roles.");
            }

            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return OperationResult.Failure("Failed to add new roles.");
            }

            await transaction.CommitAsync();
            return OperationResult.Success("User roles updated successfully.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return OperationResult.Failure("An error occurred while updating roles.");
        }
    }


}