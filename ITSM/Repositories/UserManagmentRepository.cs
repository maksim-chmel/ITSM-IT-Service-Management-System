using System.Security.Claims;
using ITSM.DB;
using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class UserManagementRepository(DBaseContext dBaseContext, UserManager<User> userManager)
    : IUserManagementRepository
{
    public async Task<User?> GetUserById(string id)
    {
        return await dBaseContext.Users.FindAsync(id);
    }

    public async Task<bool> DeleteUserById(string userId)
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

    public async Task<UserWithRolesViewModel[]> GetAllUsersToList()
    {
        var users = await dBaseContext.Users.AsNoTracking().ToListAsync();
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
        return new UserWithRolesViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = string.Join(", ", roles)
        };
    }


    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        return await userManager.GetUserAsync(principal);
    }

    public async Task EditUser(string id, EditUserViewModel editmodel)
    {
        var user = await GetUserById(id) ?? throw new ArgumentException("Пользователь не найден");

       
        user.UserName = editmodel.UserName;
        user.Email = editmodel.Email;
        user.NormalizedEmail = editmodel.Email?.Normalize();
        user.PhoneNumber = editmodel.PhoneNumber;

       
        if (!string.IsNullOrEmpty(editmodel.NewPassword) && !string.IsNullOrEmpty(editmodel.ConfirmPassword))
        {
          
            if (editmodel.NewPassword != editmodel.ConfirmPassword)
            {
                throw new ArgumentException("Новый пароль и подтверждение пароля не совпадают.");
            }

           
            var removePasswordResult = await userManager.RemovePasswordAsync(user);
            if (removePasswordResult.Succeeded)
            {
                var addPasswordResult = await userManager.AddPasswordAsync(user, editmodel.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                   
                    var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                    throw new Exception($"Не удалось изменить пароль: {errors}");
                }
            }
            else
            {
               
                var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                throw new Exception($"Не удалось удалить старый пароль: {errors}");
            }
        }
        await dBaseContext.SaveChangesAsync();
    }

    public async Task<EditUserViewModel> CreateEditUserViewModel(string userId)
    {
        var user = await GetUserById(userId) ?? throw new ArgumentException("Пользователь не найден");

        return new EditUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
    }
}