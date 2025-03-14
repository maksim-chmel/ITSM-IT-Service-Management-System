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

    public async Task DeleteUserById(string id)
    {
        var user = await GetUserById(id);
        if (user != null)
        {
            dBaseContext.Users.Remove(user);
            await dBaseContext.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetAllUsersToList()
    {
        return await dBaseContext.Users.ToListAsync();
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        return await userManager.GetUserAsync(principal);
    }

    public async Task EditUser(string id, EditUserViewModel editmodel)
    {
        var user = await GetUserById(id);
        if (user != null)
        {
            user.UserName = editmodel.UserName;
            user.Email = editmodel.Email;
            user.PhoneNumber = editmodel.PhoneNumber;
            user.Role = editmodel.Role;
        }
        await dBaseContext.SaveChangesAsync();
        
    }
}