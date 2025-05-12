using ITSM.Models;

namespace ITSM.Repositories.Archive;

public interface IArchiveService
{
    Task<IEnumerable<KnowledgeBaseArticle>> GetDeletedArticlesAsync();
    Task<IEnumerable<Models.Ticket>> GetDeletedTicketsAsync();
    Task<IEnumerable<Models.TicketCategory>> GetDeletedCategoriesAsync();
    Task<IEnumerable<User>> GetDeletedUsersAsync();
    Task RestoreArticlesAsync(List<int> selectedArticleIds);
    Task RestoreTicketsAsync(List<int> selectedTicketIds);
    Task RestoreCategoriesAsync(List<int> selectedCategoryIds);
    Task RestoreSubCategoriesAsync(List<int> selectedSubCategoryIds);
    Task RestoreUsersAsync(List<string> selectedUserIds);
}
