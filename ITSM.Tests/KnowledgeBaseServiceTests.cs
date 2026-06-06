using FluentAssertions;
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

    private async Task<User> EnsureUser(string userId)
    {
        var existing = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        if (existing != null) return existing;
        var user = new User { Id = userId, UserName = userId, Email = $"{userId}@test.com" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private async Task<KnowledgeBaseArticle> SeedArticle(string authorId = "author1")
    {
        await EnsureUser(authorId);
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
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateArticle_Fails_WhenContentIsWhitespace()
    {
        var model = new CreateKnowArtViewModel { Article = "Title", Content = "  ", CategoryId = 1 };
        var result = await _sut.CreateArticle("user1", model);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateArticle_Succeeds_AndPersistsRecord()
    {
        var model = new CreateKnowArtViewModel { Article = "How to reset", Content = "Step 1...", CategoryId = 1 };
        var result = await _sut.CreateArticle("user1", model);

        result.IsSuccess.Should().BeTrue();
        _db.KnowledgeBaseArticles.Should().HaveCount(1);
    }

    // ── UpdateArticle ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateArticle_Fails_WhenNotFound()
    {
        var model = new EditKnowBaseViewModel { Id = 999, Article = "New", Content = "New", CategoryId = 1 };
        var result = await _sut.UpdateArticle("author1", model);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateArticle_Fails_WhenCallerIsNotAuthor()
    {
        var article = await SeedArticle("author1");
        var model = new EditKnowBaseViewModel { Id = article.Id, Article = "X", Content = "Y", CategoryId = 1 };
        var result = await _sut.UpdateArticle("someone-else", model);
        result.IsSuccess.Should().BeFalse();
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

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.KnowledgeBaseArticles.FindAsync(article.Id);
        updated!.Article.Should().Be("Updated Title");
        updated.Content.Should().Be("Updated Content");
        updated.CategoryId.Should().Be(2);
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
        updated!.CreatedAt.Should().Be(originalCreatedAt);
    }

    // ── ArchiveArticle ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveArticle_Fails_WhenNotFound()
    {
        var result = await _sut.ArchiveArticle(999, "author1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ArchiveArticle_Fails_WhenCallerIsNotAuthor()
    {
        var article = await SeedArticle("author1");
        var result = await _sut.ArchiveArticle(article.Id, "someone-else");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ArchiveArticle_Succeeds_AndMarksDeleted()
    {
        var article = await SeedArticle("author1");
        var result = await _sut.ArchiveArticle(article.Id, "author1");

        result.IsSuccess.Should().BeTrue();
        var archived = await _db.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == article.Id);
        archived!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveArticle_HidesArticleFromNormalQueries()
    {
        var article = await SeedArticle("author1");
        await _sut.ArchiveArticle(article.Id, "author1");

        _db.ChangeTracker.Clear();
        var found = await _db.KnowledgeBaseArticles.FindAsync(article.Id);
        found.Should().BeNull();
    }

    // ── GetAllArticlesByCategory ───────────────────────────────────────────────

    [Fact]
    public async Task GetAllArticlesByCategory_ReturnsAllCategories_WithArticlesIncluded()
    {
        await SeedArticle("author1"); // CategoryId = 1

        var result = await _sut.GetAllArticlesByCategory();

        result.Should().HaveCount(5); // 5 seed categories
        result.Should().Contain(c => c.Id == 1 && c.Articles.Count == 1);
    }

    [Fact]
    public async Task GetAllArticlesByCategory_ReturnsCategories_WhenNoArticlesExist()
    {
        var result = await _sut.GetAllArticlesByCategory();

        result.Should().HaveCount(5);
        result.Should().OnlyContain(c => c.Articles.Count == 0);
    }

    // ── GetArticleById ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetArticleById_ReturnsArticle_WhenFound()
    {
        var article = await SeedArticle();

        var result = await _sut.GetArticleById(article.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task GetArticleById_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetArticleById(999);
        result.Should().BeNull();
    }

    // ── GetAllAuthorArticles ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAuthorArticles_ReturnsOnlyArticlesByGivenAuthor()
    {
        await SeedArticle("author1");
        await SeedArticle("author2");

        var result = await _sut.GetAllAuthorArticles("author1");

        result.Should().HaveCount(1)
              .And.OnlyContain(a => a.AuthorId == "author1");
    }

    [Fact]
    public async Task GetAllAuthorArticles_ReturnsEmpty_WhenAuthorHasNoArticles()
    {
        await SeedArticle("author1");

        var result = await _sut.GetAllAuthorArticles("other");

        result.Should().BeEmpty();
    }
}
