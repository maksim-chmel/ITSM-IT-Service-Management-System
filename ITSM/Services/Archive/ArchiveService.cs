using ITSM.Data;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Archive;

public class ArchiveService(DBaseContext context, UserManager<User> userManager) : IArchiveService
{
    public async Task<IEnumerable<KnowledgeBaseArticle>> GetDeletedArticlesAsync()
    {
        return await context.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .Where(a => a.IsDeleted)
            .Include(a=>a.Category)
            .Include(a => a.Author)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetDeletedTicketsAsync()
    {
        return await context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted)
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.TicketCategory>> GetDeletedCategoriesAsync()
    {
        return await context.TicketCategories
            .IgnoreQueryFilters()
            .Include(c => c.SubCategories)
            .Where(c => c.IsDeleted || c.SubCategories.Any(sc => sc.IsDeleted))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetDeletedUsersAsync()
    {
        return await context.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Discussion>> GetArchivedDiscussionsAsync(string? search = null)
    {
        var query = context.Discussions
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted)
            .Include(d => d.Author)
            .Include(d => d.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(search));
        }

        return await query.ToListAsync();
    }
    
    public async Task<OperationResult> RestoreUsersAsync(List<string> selectedUserIds)
    {
        if (selectedUserIds == null || selectedUserIds.Count == 0)
            return OperationResult.Failure("No users selected for restoration.");

        int count = 0;
        foreach (var userId in selectedUserIds)
        {
            var user = await userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null || !user.IsDeleted) continue;

            user.IsDeleted = false;
            user.LockoutEnabled = false;
            user.LockoutEnd = null;
            await userManager.UpdateAsync(user);
            count++;
        }

        return OperationResult.Success($"{count} users restored successfully.");
    }

    public async Task<OperationResult> RestoreEntitiesAsync<T>(DbSet<T> dbSet, List<int> selectedIds) where T : class, ISoftDeletable
    {
        if (selectedIds == null || selectedIds.Count == 0)
            return OperationResult.Failure("No items selected for restoration.");

        var entities = await dbSet
            .IgnoreQueryFilters()
            .Where(e => selectedIds.Contains(Microsoft.EntityFrameworkCore.EF.Property<int>(e, "Id")) && e.IsDeleted)
            .ToListAsync();

        if (entities.Count == 0)
            return OperationResult.Failure("No deleted items found to restore.");

        foreach (var entity in entities)
        {
            entity.IsDeleted = false;
        }

        await context.SaveChangesAsync();
        return OperationResult.Success($"{entities.Count} items restored successfully.");
    }

}
