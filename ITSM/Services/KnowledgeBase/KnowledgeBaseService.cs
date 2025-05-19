using ITSM.DB;
using ITSM.Models;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.KnowledgeBase;

public class KnowledgeBaseService(DBaseContext dBaseContext) : IKnowledgeBaseService
{
    
    public async Task<List<Models.TicketCategory>> GetAllArticlesByCategory()
    {
        var categories = await dBaseContext.TicketCategories
            .Where(c => !c.IsDeleted)
            .Include(c => c.Articles)
            .ThenInclude(a => a.Author)
            .ToListAsync();
        
        foreach (var category in categories)
        {
            category.Articles = category.Articles
                .Where(a => !a.IsDeleted)
                .ToList();
        }

        return categories;
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
            .Where(c => !c.IsDeleted)
            .Include(a => a.Author)
            .Include(a => a.Category)
            .ToListAsync();
    }

    public async Task<bool> CreateArticle(string userId,CreateKnowArtViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Article) || string.IsNullOrWhiteSpace(viewModel.Content)) return false;
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
        return true;
    }

    public async Task<bool> DeleteArticle(int articleId, string authorId)
    {
        var article = await dBaseContext.KnowledgeBaseArticles.FindAsync(articleId);
        if (article == null || article.AuthorId != authorId) return false;

        article.IsDeleted = true;
        await dBaseContext.SaveChangesAsync();
        return true;
    }


    public async Task<bool> UpdateArticle(string authorId, EditKnowBaseViewModel viewModel)
    {
        var article = await dBaseContext.KnowledgeBaseArticles.FindAsync(viewModel.Id);
        if (article == null || article.AuthorId != authorId) return false;
        article.Article = viewModel.Article;
        article.Content = viewModel.Content;
        article.CategoryId = viewModel.CategoryId;
        article.CreatedAt = DateTime.Now;
        dBaseContext.KnowledgeBaseArticles.Update(article);
        await dBaseContext.SaveChangesAsync();
        return true;

    }
}