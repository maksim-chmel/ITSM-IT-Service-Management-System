using ITSM.Models;
using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.KnowledgeBase;

public interface IKnowledgeBaseRepository
{
    Task<List<KnowledgeBaseArticle>> GetAllAuthorArticles(string authorId);
    Task CreateArticle(string userId, CreateKnowArtViewModel viewModel);
    Task DeleteArticle(int articleId, string authorId);
    Task UpdateArticle(string authorId, EditKnowBaseViewModel viewModel);
    Task<List<Models.TicketCategory>> GetAllArticlesByCategory();
    Task<KnowledgeBaseArticle> GetArticleById(int id);

}