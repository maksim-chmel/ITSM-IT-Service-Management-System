using ITSM.DB;
using ITSM.Models;
using ITSM.ViewModels.Manage;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.KnowledgeBase;

public class KnowledgeBaseRepository(DBaseContext dBaseContext) : IKnowledgeBaseRepository
{
    
    public async Task<List<Models.TicketCategory>> GetAllArticlesByCategory()
    {
        return await dBaseContext.TicketCategories
            .Include(c => c.Articles)
            .ThenInclude(a => a.Author)
            .ToListAsync();
    }

    public async Task<KnowledgeBaseArticle> GetArticleById(int id)
    {
        return await dBaseContext.KnowledgeBaseArticles
            .Include(c => c.Author)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<KnowledgeBaseArticle>> GetAllAuthorArticles(string authorId)
    {
        return await dBaseContext.KnowledgeBaseArticles
            .Where(a => a.AuthorId == authorId)
            .Include(a => a.Author)
            .Include(a => a.Category)
            .ToListAsync();
    }

    public async Task CreateArticle(string userId,CreateKnowArtViewModel viewModel)
    {
        var newArticle = new KnowledgeBaseArticle
        {
            Article = viewModel.Article,
            Content = viewModel.Content,
            AuthorId = userId,
            CreatedAt = DateTime.Now,
            CategoryId = viewModel.CategoryId
        };
        await dBaseContext.KnowledgeBaseArticles.AddAsync(newArticle);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task DeleteArticle(int articleId, string authorId)
    {
        var article = await dBaseContext.KnowledgeBaseArticles.FindAsync(articleId);
        if (article != null && article.AuthorId == authorId)
        {
            dBaseContext.KnowledgeBaseArticles.Remove(article);
            await dBaseContext.SaveChangesAsync();
        }
    }

    public async Task UpdateArticle(string authorId, EditKnowBaseViewModel viewModel)
    {
        var article = await dBaseContext.KnowledgeBaseArticles.FindAsync(viewModel.Id);
        if (article != null && article.AuthorId == authorId)
        {
            article.Article = viewModel.Article;
            article.Content = viewModel.Content;
            article.CategoryId = viewModel.CategoryId;
            article.CreatedAt = DateTime.Now;
            dBaseContext.KnowledgeBaseArticles.Update(article);
            await dBaseContext.SaveChangesAsync();
        }
    }
}