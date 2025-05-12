using ITSM.DB;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.Archive;

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

    public async Task RestoreArticlesAsync(List<int> selectedArticleIds)
    {
        var articles = await context.KnowledgeBaseArticles
            .Where(a => selectedArticleIds.Contains(a.Id) && a.IsDeleted)
            .ToListAsync();

        foreach (var article in articles)
        {
            article.IsDeleted = false;
        }

        await context.SaveChangesAsync();
    }

    public async Task RestoreTicketsAsync(List<int> selectedTicketIds)
    {
        var tickets = await context.Tickets
            .Where(t => selectedTicketIds.Contains(t.Id) && t.IsDeleted)
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            ticket.IsDeleted = false;
        }

        await context.SaveChangesAsync();
    }

    public async Task RestoreCategoriesAsync(List<int> selectedCategoryIds)
    {
        var categories = await context.TicketCategories
            .Where(c => selectedCategoryIds.Contains(c.Id) && c.IsDeleted)
            .ToListAsync();

        foreach (var category in categories)
        {
            category.IsDeleted = false;
        }

        await context.SaveChangesAsync();
    }

    public async Task RestoreSubCategoriesAsync(List<int> selectedSubCategoryIds)
    {
        var subCategories = await context.TicketSubCategories
            .Where(sc => selectedSubCategoryIds.Contains(sc.Id) && sc.IsDeleted)
            .ToListAsync();

        foreach (var sub in subCategories)
        {
            sub.IsDeleted = false;
        }

        await context.SaveChangesAsync();
    }

    public async Task RestoreUsersAsync(List<string> selectedUserIds)
    {
        foreach (var userId in selectedUserIds)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted) continue;

            user.IsDeleted = false;
            user.LockoutEnabled = false;
            await userManager.UpdateAsync(user);
        }
    }
}
