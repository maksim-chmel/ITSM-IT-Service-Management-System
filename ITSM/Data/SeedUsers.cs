using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Data;

public static class SeedUsers
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        var usersToSeed = new (string UserName, string Email, string Password, string Role)[]
        {
            ("admin", "admin@itsm.local", "Admin123!", nameof(UserRoles.Admin)),
            ("coordinator", "coordinator@itsm.local", "Coordinator123!", nameof(UserRoles.Coordinator)),
            ("technician", "technician@itsm.local", "Technician123!", nameof(UserRoles.Technician)),
            ("user", "user@itsm.local", "User123!", nameof(UserRoles.User))
        };

        foreach (var seedUser in usersToSeed)
        {
            var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
            if (existingUser != null)
            {
                if (!await userManager.IsInRoleAsync(existingUser, seedUser.Role))
                {
                    await userManager.AddToRoleAsync(existingUser, seedUser.Role);
                }

                continue;
            }

            var user = new User
            {
                UserName = seedUser.UserName,
                Email = seedUser.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, seedUser.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed user '{seedUser.Email}': {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, seedUser.Role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to assign role '{seedUser.Role}' to '{seedUser.Email}': {errors}");
            }
        }
    }
}
