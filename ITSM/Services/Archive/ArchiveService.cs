using ITSM.DB;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Archive;

public class ArchiveService(DBaseContext context, UserManager<User> userManager) : IArchiveService
{
    public async Task<IEnumerable<KnowledgeBaseArticle>> GetDeletedArticlesAsync()
    {
        return await context.KnowledgeBaseArticles
            .Where(a => a.IsDeleted)
            .Include(a=>a.Category)
            .Include(a => a.Author)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetDeletedTicketsAsync()
    {
        return await context.Tickets
            .Where(t => t.IsDeleted)
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.TicketCategory>> GetDeletedCategoriesAsync()
    {
        return await context.TicketCategories
            .Include(c => c.SubCategories)
            .Where(c => c.IsDeleted || c.SubCategories.Any(sc => sc.IsDeleted))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetDeletedUsersAsync()
    {
        return await context.Users
            .Where(u => u.IsDeleted)
            .ToListAsync();
    }
    
    public async Task<bool> RestoreUsersAsync(List<string> selectedUserIds)
    {
        if (selectedUserIds == null || selectedUserIds.Count == 0)
            return false;
        foreach (var userId in selectedUserIds)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted) continue;

            user.IsDeleted = false;
            user.LockoutEnabled = false;
            await userManager.UpdateAsync(user);
        }

        return true;
    }
    public async Task<bool> RestoreEntitiesAsync<T>(DbSet<T> dbSet, List<int> selectedIds) where T : class, ISoftDeletableEntity
    {
        if (selectedIds == null || selectedIds.Count == 0)
            return false;

        var entities = await dbSet
            .Where(e => selectedIds.Contains(e.Id) && e.IsDeleted)
            .ToListAsync();

        if (entities.Count == 0)
            return false;

        foreach (var entity in entities)
        {
            entity.IsDeleted = false;
        }

        await context.SaveChangesAsync();
        return true;
    }

}
