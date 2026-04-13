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
        return await dBaseContext.Users.FindAsync(id);
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
    public async Task<bool> SoftDeleteUserById(string userId)
    {
        await using var transaction = await dBaseContext.Database.BeginTransactionAsync();
        var user = await dBaseContext.Users
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;
        user.IsDeleted = true;
        user.LockoutEnabled = true;

        dBaseContext.Users.Update(user);
        await dBaseContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }


    public async Task<UserWithRolesViewModel[]> GetAllUsersToList()
    {
        var users = await dBaseContext.Users
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        var userWithRolesList = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            var userWithRoles = await GetUserWithRoles(user);
            userWithRolesList.Add(userWithRoles);
        }

        return userWithRolesList.ToArray();
    }

    private async Task<UserWithRolesViewModel> GetUserWithRoles(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var assignedCategories = await dBaseContext.UserCategoryAssignments
            .Where(uca => uca.UserId == user.Id)
            .Join(dBaseContext.TicketCategories,
                uca => uca.CategoryId,
                cat => cat.Id,
                (uca, cat) => cat.Name)
            .ToListAsync();

        return new UserWithRolesViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList(),
            AssignedCategories = assignedCategories,
            IsDeleted = user.IsDeleted
        };
    }


    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        return await userManager.GetUserAsync(principal);
    }

    public async Task EditUser(string id, EditUserViewModel editmodel)
    {
        var user = await GetUserById(id) ?? throw new ArgumentException("User not found.");

       
        user.UserName = editmodel.UserName;
        user.Email = editmodel.Email;
        user.NormalizedEmail = editmodel.Email?.Normalize();
        user.PhoneNumber = editmodel.PhoneNumber;

       
        if (!string.IsNullOrEmpty(editmodel.NewPassword) && !string.IsNullOrEmpty(editmodel.ConfirmPassword))
        {
          
            if (editmodel.NewPassword != editmodel.ConfirmPassword)
            {
                throw new ArgumentException("The new password and confirmation password do not match.");
            }

           
            var removePasswordResult = await userManager.RemovePasswordAsync(user);
            if (removePasswordResult.Succeeded)
            {
                var addPasswordResult = await userManager.AddPasswordAsync(user, editmodel.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                   
                    var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to change password: {errors}");
                }
            }
            else
            {
               
                var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to remove the old password: {errors}");
            }
        }
        await dBaseContext.SaveChangesAsync();
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
        var allUsers = await GetAllUsersToList();

        if (string.IsNullOrWhiteSpace(search))
            return allUsers.ToArray();

        search = search.ToLower();

        var filtered = allUsers
            .Where(u =>
                (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||
                (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(search, StringComparison.CurrentCultureIgnoreCase)))
            .ToArray();

        return filtered;
    }
}
