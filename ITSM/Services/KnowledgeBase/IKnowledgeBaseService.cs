using ITSM.Models;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;

namespace ITSM.Services.KnowledgeBase;

public interface IKnowledgeBaseService
{
    Task<List<KnowledgeBaseArticle>> GetAllAuthorArticles(string authorId);
    Task<bool> CreateArticle(string userId, CreateKnowArtViewModel viewModel);
    Task<bool> DeleteArticle(int articleId, string authorId);
    Task<bool> UpdateArticle(string authorId, EditKnowBaseViewModel viewModel);
    Task<List<Models.TicketCategory>> GetAllArticlesByCategory();
    Task<KnowledgeBaseArticle> GetArticleById(int id);

}