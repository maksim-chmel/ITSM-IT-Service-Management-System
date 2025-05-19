using ITSM.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Archive;

public interface IArchiveService
{
    Task<IEnumerable<KnowledgeBaseArticle>> GetDeletedArticlesAsync();
    Task<IEnumerable<Models.Ticket>> GetDeletedTicketsAsync();
    Task<IEnumerable<Models.TicketCategory>> GetDeletedCategoriesAsync();
    Task<IEnumerable<User>> GetDeletedUsersAsync();
    Task<bool> RestoreUsersAsync(List<string> selectedUserIds);
    Task<bool> RestoreEntitiesAsync<T>(DbSet<T> dbSet, List<int> selectedIds) where T : class, ISoftDeletableEntity;
}
