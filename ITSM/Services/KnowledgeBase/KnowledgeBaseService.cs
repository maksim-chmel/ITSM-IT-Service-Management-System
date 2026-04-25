using ITSM.Data;
using ITSM.Models;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.KnowledgeBase;

public class KnowledgeBaseService(DBaseContext dBaseContext) : IKnowledgeBaseService
{
    
    public async Task<List<Models.TicketCategory>> GetAllArticlesByCategory()
    {
        return await dBaseContext.TicketCategories
            .Include(c => c.Articles)
            .ThenInclude(a => a.Author)
            .ToListAsync();
    }


    public async Task<KnowledgeBaseArticle?> GetArticleById(int id)
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

    public async Task<OperationResult> CreateArticle(string userId,CreateKnowArtViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Article) || string.IsNullOrWhiteSpace(viewModel.Content)) 
            return OperationResult.Failure("Article title and content are required.");
            
        var newArticle = new KnowledgeBaseArticle
        {
            Article = viewModel.Article,
            Content = viewModel.Content,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow,
            CategoryId = viewModel.CategoryId
        };
        await dBaseContext.KnowledgeBaseArticles.AddAsync(newArticle);
        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("Knowledge base article created successfully.");
    }

    public async Task<OperationResult> ArchiveArticle(int articleId, string authorId)
    {
        var article = await dBaseContext.KnowledgeBaseArticles.FindAsync(articleId);
        if (article == null) return OperationResult.Failure("Article not found.");
        if (article.AuthorId != authorId) return OperationResult.Failure("You are not the author of this article.");

        article.IsDeleted = true;
        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("Article archived successfully.");
    }


    public async Task<OperationResult> UpdateArticle(string authorId, EditKnowBaseViewModel viewModel)
    {
        var article = await dBaseContext.KnowledgeBaseArticles.FindAsync(viewModel.Id);
        if (article == null) return OperationResult.Failure("Article not found.");
        if (article.AuthorId != authorId) return OperationResult.Failure("You are not the author of this article.");
        
        article.Article = viewModel.Article;
        article.Content = viewModel.Content;
        article.CategoryId = viewModel.CategoryId;
        article.CreatedAt = DateTime.UtcNow;
        dBaseContext.KnowledgeBaseArticles.Update(article);
        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("Article updated successfully.");

    }
}
