using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Models;
using ITSM.Services.KnowledgeBase;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;

namespace ITSM.Tests;

public class KnowledgeBaseServiceTests
{
    private readonly DBaseContext _db;
    private readonly KnowledgeBaseService _sut;

    public KnowledgeBaseServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();
        _sut = new KnowledgeBaseService(_db);
    }

    private async Task<KnowledgeBaseArticle> SeedArticle(string authorId = "author1")
    {
        var article = new KnowledgeBaseArticle
        {
            Article = "Title",
            Content = "Content",
            AuthorId = authorId,
            CategoryId = 1,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _db.KnowledgeBaseArticles.Add(article);
        await _db.SaveChangesAsync();
        return article;
    }

    // ── CreateArticle ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateArticle_Fails_WhenTitleIsEmpty()
    {
        var model = new CreateKnowArtViewModel { Article = "", Content = "Content", CategoryId = 1 };
        var result = await _sut.CreateArticle("user1", model);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateArticle_Fails_WhenContentIsWhitespace()
    {
        var model = new CreateKnowArtViewModel { Article = "Title", Content = "  ", CategoryId = 1 };
        var result = await _sut.CreateArticle("user1", model);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateArticle_Succeeds_AndPersistsRecord()
    {
        var model = new CreateKnowArtViewModel { Article = "How to reset", Content = "Step 1...", CategoryId = 1 };
        var result = await _sut.CreateArticle("user1", model);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _db.KnowledgeBaseArticles.Count());
    }

    // ── UpdateArticle ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateArticle_Fails_WhenNotFound()
    {
        var model = new EditKnowBaseViewModel { Id = 999, Article = "New", Content = "New", CategoryId = 1 };
        var result = await _sut.UpdateArticle("author1", model);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateArticle_Fails_WhenCallerIsNotAuthor()
    {
        var article = await SeedArticle("author1");
        var model = new EditKnowBaseViewModel { Id = article.Id, Article = "X", Content = "Y", CategoryId = 1 };
        var result = await _sut.UpdateArticle("someone-else", model);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateArticle_Succeeds_AndUpdatesFields()
    {
        var article = await SeedArticle("author1");
        var model = new EditKnowBaseViewModel
        {
            Id = article.Id,
            Article = "Updated Title",
            Content = "Updated Content",
            CategoryId = 2
        };
        var result = await _sut.UpdateArticle("author1", model);

        Assert.True(result.IsSuccess);
        var updated = await _db.KnowledgeBaseArticles.FindAsync(article.Id);
        Assert.Equal("Updated Title", updated!.Article);
        Assert.Equal("Updated Content", updated.Content);
        Assert.Equal(2, updated.CategoryId);
    }

    [Fact]
    public async Task UpdateArticle_DoesNotOverwriteCreatedAt()
    {
        var article = await SeedArticle("author1");
        var originalCreatedAt = article.CreatedAt;

        var model = new EditKnowBaseViewModel
        {
            Id = article.Id,
            Article = "Changed",
            Content = "Changed",
            CategoryId = 1
        };
        await _sut.UpdateArticle("author1", model);

        var updated = await _db.KnowledgeBaseArticles.FindAsync(article.Id);
        Assert.Equal(originalCreatedAt, updated!.CreatedAt);
    }

    // ── ArchiveArticle ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveArticle_Fails_WhenNotFound()
    {
        var result = await _sut.ArchiveArticle(999, "author1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ArchiveArticle_Fails_WhenCallerIsNotAuthor()
    {
        var article = await SeedArticle("author1");
        var result = await _sut.ArchiveArticle(article.Id, "someone-else");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ArchiveArticle_Succeeds_AndMarksDeleted()
    {
        var article = await SeedArticle("author1");
        var result = await _sut.ArchiveArticle(article.Id, "author1");

        Assert.True(result.IsSuccess);
        var archived = await _db.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == article.Id);
        Assert.True(archived!.IsDeleted);
    }

    [Fact]
    public async Task ArchiveArticle_HidesArticleFromNormalQueries()
    {
        var article = await SeedArticle("author1");
        await _sut.ArchiveArticle(article.Id, "author1");

        _db.ChangeTracker.Clear();
        var found = await _db.KnowledgeBaseArticles.FindAsync(article.Id);
        Assert.Null(found);
    }
}
