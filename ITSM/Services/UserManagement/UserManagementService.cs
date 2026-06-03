using System.Security.Claims;
using ITSM.Data;
using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.UserManagement;

public class UserManagementService(DBaseContext dBaseContext, UserManager<User> userManager)
    : IUserManagementService
{
    public async Task<User?> GetUserById(string id)
    {
        return await dBaseContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /*  public async Task<bool> DeleteUserById(string userId)
    {
        await using var transaction = await dBaseContext.Database.BeginTransactionAsync();
        var user = await dBaseContext.Users
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;


        foreach (var ticket in user.CreatedTickets)
        {
            ticket.AuthorId = null;
        }

        foreach (var ticket in user.AssignedTickets)
        {
            ticket.AssignedUserId = null;
        }

        dBaseContext.Users.Remove(user);
        await dBaseContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
    */
    public async Task<OperationResult> SoftDeleteUserById(string userId)
    {
        var user = await dBaseContext.Users
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return OperationResult.Failure("User not found.");

        user.IsDeleted = true;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        // Invalidate current sessions
        await userManager.UpdateSecurityStampAsync(user);
        
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
            return OperationResult.Success("User archived and sessions invalidated.");
            
        return OperationResult.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }


    public async Task<UserWithRolesViewModel[]> GetAllUsersToList()
    {
        var users = await dBaseContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(u => u.UserCategoryAssignments)
                .ThenInclude(uca => uca.TicketCategory)
            .ToListAsync();

        return await BuildUserViewModelsAsync(users);
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        return await userManager.GetUserAsync(principal);
    }

    public async Task<OperationResult> EditUser(string id, EditUserViewModel editmodel)
    {
        var user = await GetUserById(id);
        if (user == null) return OperationResult.Failure("User not found.");

        if (!string.IsNullOrWhiteSpace(editmodel.Email) &&
            !string.Equals(user.Email, editmodel.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await userManager.FindByEmailAsync(editmodel.Email);
            if (existing != null && existing.Id != id)
                return OperationResult.Failure("This email address is already in use.");
        }

        user.UserName = editmodel.UserName;
        user.NormalizedUserName = userManager.NormalizeName(editmodel.UserName);
        user.Email = editmodel.Email;
        user.NormalizedEmail = userManager.NormalizeEmail(editmodel.Email);
        user.PhoneNumber = editmodel.PhoneNumber;

       
        if (!string.IsNullOrEmpty(editmodel.NewPassword) && !string.IsNullOrEmpty(editmodel.ConfirmPassword))
        {
          
            if (editmodel.NewPassword != editmodel.ConfirmPassword)
            {
                return OperationResult.Failure("The new password and confirmation password do not match.");
            }

           
            var removePasswordResult = await userManager.RemovePasswordAsync(user);
            if (removePasswordResult.Succeeded)
            {
                var addPasswordResult = await userManager.AddPasswordAsync(user, editmodel.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                    return OperationResult.Failure($"Failed to change password: {errors}");
                }
            }
            else
            {
                var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                return OperationResult.Failure($"Failed to remove the old password: {errors}");
            }
        }
        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("User profile updated successfully.");
    }

    public async Task<EditUserViewModel> CreateEditUserViewModel(string userId)
    {
        var user = await GetUserById(userId) ?? throw new ArgumentException("User not found.");

        return new EditUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
    }

    public async Task<UserWithRolesViewModel[]> SearchUser(string? search)
    {
        var query = dBaseContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(u => u.UserCategoryAssignments)
                .ThenInclude(uca => uca.TicketCategory);

        List<User> users;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            users = await query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(s)) ||
                (u.Email != null && u.Email.ToLower().Contains(s)))
                .ToListAsync();
        }
        else
        {
            users = await query.ToListAsync();
        }

        return await BuildUserViewModelsAsync(users);
    }

    private async Task<UserWithRolesViewModel[]> BuildUserViewModelsAsync(List<User> users)
    {
        var userIds = users.Select(u => u.Id).ToList();

        var rolePairs = await (
            from ur in dBaseContext.UserRoles
            join r in dBaseContext.Roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync();

        var rolesByUser = rolePairs
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name!).ToList());

        return users.Select(user => new UserWithRolesViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsDeleted = user.IsDeleted,
            Roles = rolesByUser.GetValueOrDefault(user.Id) ?? [],
            AssignedCategories = user.UserCategoryAssignments
                .Select(uca => uca.TicketCategory.Name)
                .ToList()
        }).ToArray();
    }
}
