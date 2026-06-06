using Moq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Models;
using ITSM.Services.Archive;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Tests;

public class ArchiveServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly Mock<UserManager<User>> _userManager;

    public ArchiveServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

       
        using var pragma = _connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = OFF;";
        pragma.ExecuteNonQuery();
        
        using var db = CreateDb();
        db.Database.EnsureCreated();

        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _userManager
            .Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    public void Dispose() => _connection.Dispose();
    
    private DBaseContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseSqlite(_connection)
            .Options;
        return new DBaseContext(options);
    }

    private ArchiveService CreateSut() => new(CreateDb(), _userManager.Object);
    private ArchiveService CreateSut(DBaseContext db) => new(db, _userManager.Object);

    private async Task SeedTicket(bool isDeleted = false)
    {
        await using var db = CreateDb();
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "u", Email = "u@t.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var ticket = new Ticket { Title = "T", Description = "D", CategoryId = 1, AuthorId = user.Id };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        if (isDeleted)
        {
            ticket.IsDeleted = true;
            await db.SaveChangesAsync();
        }
    }

    private async Task SeedUser(bool isDeleted = false)
    {
        await using var db = CreateDb();
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "u",
            Email = "u@test.com",
            IsDeleted = isDeleted
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private async Task SeedArticle(bool isDeleted = false)
    {
        await using var db = CreateDb();
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "u", Email = "u@t.com" };
        db.Users.Add(user);
        var cat = new TicketCategory { Name = "Cat" };
        db.TicketCategories.Add(cat);
        await db.SaveChangesAsync();
        db.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
        {
            Article = "A", Content = "C",
            AuthorId = user.Id, CategoryId = cat.Id,
            IsDeleted = isDeleted
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedCategory(bool isDeleted = false, bool withDeletedSubcat = false)
    {
        await using var db = CreateDb();
        var cat = new TicketCategory { Name = "Cat", IsDeleted = isDeleted };
        db.TicketCategories.Add(cat);
        await db.SaveChangesAsync();
        if (withDeletedSubcat)
        {
            db.TicketSubCategories.Add(new TicketSubCategory
            {
                Name = "Sub", CategoryId = cat.Id, IsDeleted = true
            });
            await db.SaveChangesAsync();
        }
    }

    private async Task SeedDiscussion(bool isDeleted = false, string title = "T")
    {
        await using var db = CreateDb();
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "u", Email = "u@t.com" };
        db.Users.Add(user);
        var cat = new TicketCategory { Name = "Cat" };
        db.TicketCategories.Add(cat);
        await db.SaveChangesAsync();
        db.Discussions.Add(new Discussion
        {
            Title = title, Description = "D",
            AuthorId = user.Id, CategoryId = cat.Id,
            IsDeleted = isDeleted
        });
        await db.SaveChangesAsync();
    }

  

    [Fact]
    public async Task GetDeletedTicketsAsync_ReturnsOnlyDeletedTickets()
    {
        await SeedTicket(isDeleted: true);
        await SeedTicket(isDeleted: false);

        var result = await CreateSut().GetDeletedTicketsAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDeletedTicketsAsync_ReturnsEmpty_WhenNoneDeleted()
    {
        await SeedTicket(isDeleted: false);

        var result = await CreateSut().GetDeletedTicketsAsync();

        result.Should().BeEmpty();
    }

   

    [Fact]
    public async Task GetDeletedUsersAsync_ReturnsOnlyDeletedUsers()
    {
        await SeedUser(true);

        var result = await CreateSut().GetDeletedUsersAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDeletedUsersAsync_ReturnsEmpty_WhenNoneDeleted()
    {
        await SeedUser(false);

        var result = await CreateSut().GetDeletedUsersAsync();

        result.Should().BeEmpty();
    }

   

    [Fact]
    public async Task GetDeletedArticlesAsync_ReturnsOnlyDeletedArticles()
    {
        await SeedArticle(isDeleted: true);
        await SeedArticle(isDeleted: false);

        var result = await CreateSut().GetDeletedArticlesAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDeletedArticlesAsync_ReturnsEmpty_WhenNoneDeleted()
    {
        await SeedArticle(isDeleted: false);

        var result = await CreateSut().GetDeletedArticlesAsync();

        result.Should().BeEmpty();
    }

    // ── GetDeletedCategoriesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetDeletedCategoriesAsync_ReturnsDeletedCategory()
    {
        await SeedCategory(isDeleted: true);

        var result = await CreateSut().GetDeletedCategoriesAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDeletedCategoriesAsync_ReturnsCategoryWithDeletedSubcategory()
    {
        await SeedCategory(isDeleted: false, withDeletedSubcat: true);

        var result = await CreateSut().GetDeletedCategoriesAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDeletedCategoriesAsync_ReturnsEmpty_WhenNoneDeleted()
    {
        await SeedCategory(isDeleted: false);

        var result = await CreateSut().GetDeletedCategoriesAsync();

        result.Should().BeEmpty();
    }

   

    [Fact]
    public async Task GetArchivedDiscussionsAsync_ReturnsOnlyArchivedDiscussions()
    {
        await SeedDiscussion(isDeleted: true);
        await SeedDiscussion(isDeleted: false);

        var result = await CreateSut().GetArchivedDiscussionsAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetArchivedDiscussionsAsync_ReturnsEmpty_WhenNoneDeleted()
    {
        await SeedDiscussion(isDeleted: false);

        var result = await CreateSut().GetArchivedDiscussionsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetArchivedDiscussionsAsync_FiltersByTitle_WhenSearchProvided()
    {
        await SeedDiscussion(isDeleted: true, title: "Alpha");
        await SeedDiscussion(isDeleted: true, title: "Beta");

        var result = await CreateSut().GetArchivedDiscussionsAsync("alp");

        result.Should().HaveCount(1)
              .And.Contain(d => d.Title == "Alpha");
    }

   

    [Fact]
    public async Task RestoreUsersAsync_ReturnsSuccess_AndCallsUpdateAsync()
    {
        await SeedUser(true);

        await using var db = CreateDb();
        var user = await db.Users.IgnoreQueryFilters().FirstAsync();
        _userManager.Setup(m => m.Users).Returns(db.Users);

        var result = await CreateSut().RestoreUsersAsync(new List<string> { user.Id });

        result.IsSuccess.Should().BeTrue();
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RestoreUsersAsync_ReturnsFailure_WhenEmptyList()
    {
        var result = await CreateSut().RestoreUsersAsync(new List<string>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreUsersAsync_SkipsUser_WhenNotDeleted()
    {
        await SeedUser(false);

        await using var db = CreateDb();
        var user = await db.Users.IgnoreQueryFilters().FirstAsync();
        _userManager.Setup(m => m.Users).Returns(db.Users);

        var result = await CreateSut().RestoreUsersAsync(new List<string> { user.Id });

        result.IsSuccess.Should().BeTrue();
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    

    [Fact]
    public async Task RestoreEntitiesAsync_RestoresDeletedEntity()
    {
        await SeedArticle(isDeleted: true);

        await using var db = CreateDb();
        var article = await db.KnowledgeBaseArticles.IgnoreQueryFilters().FirstAsync();

        var result = await CreateSut(db).RestoreEntitiesAsync(db.KnowledgeBaseArticles, new List<int> { article.Id });

        result.IsSuccess.Should().BeTrue();
        var restored = await db.KnowledgeBaseArticles.IgnoreQueryFilters().FirstAsync(a => a.Id == article.Id);
        restored.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreEntitiesAsync_ReturnsFailure_WhenEmptyList()
    {
        await using var db = CreateDb();

        var result = await CreateSut(db).RestoreEntitiesAsync(db.KnowledgeBaseArticles, new List<int>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreEntitiesAsync_ReturnsFailure_WhenNoDeletedItemsFound()
    {
        await SeedArticle(isDeleted: false);

        await using var db = CreateDb();
        var article = await db.KnowledgeBaseArticles.IgnoreQueryFilters().FirstAsync();

        var result = await CreateSut(db).RestoreEntitiesAsync(db.KnowledgeBaseArticles, new List<int> { article.Id });

        result.IsSuccess.Should().BeFalse();
    }
}
