using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Discussion;
using ITSM.ViewModels.Create;

namespace ITSM.Tests;

public class DiscussionServiceTests
{
    private readonly DBaseContext _db;
    private readonly DiscussionService _sut;

    public DiscussionServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();
        _sut = new DiscussionService(_db);
    }

    private async Task<Discussion> SeedDiscussion(string authorId = "user1", Status status = Status.Open)
    {
        var d = new Discussion
        {
            Title = "Topic",
            Description = "Details",
            AuthorId = authorId,
            CategoryId = 1,
            Status = status
        };
        _db.Discussions.Add(d);
        await _db.SaveChangesAsync();
        return d;
    }

    // ── CreateDiscussion ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateDiscussion_Fails_WhenTitleIsEmpty()
    {
        var model = new DiscussionCreateViewModel { Title = "", Description = "Desc", CategoryId = 1 };
        var result = await _sut.CreateDiscussion(model, "user1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateDiscussion_Fails_WhenDescriptionIsWhitespace()
    {
        var model = new DiscussionCreateViewModel { Title = "Title", Description = "  ", CategoryId = 1 };
        var result = await _sut.CreateDiscussion(model, "user1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateDiscussion_Succeeds_AndPersistsRecord()
    {
        var model = new DiscussionCreateViewModel { Title = "My Topic", Description = "Details", CategoryId = 1 };
        var result = await _sut.CreateDiscussion(model, "user1");

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _db.Discussions.Count());
    }

    // ── ResolveDiscussion ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveDiscussion_Fails_WhenNotFound()
    {
        var result = await _sut.ResolveDiscussion(999, "user1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResolveDiscussion_Fails_WhenCallerIsNotAuthor()
    {
        var d = await SeedDiscussion("author1");
        var result = await _sut.ResolveDiscussion(d.Id, "someone-else");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResolveDiscussion_Succeeds_AndSetsResolvedStatusAndClosedAt()
    {
        var d = await SeedDiscussion("author1");
        var result = await _sut.ResolveDiscussion(d.Id, "author1");

        Assert.True(result.IsSuccess);
        var updated = await _db.Discussions.FirstAsync(x => x.Id == d.Id);
        Assert.Equal(Status.Resolved, updated.Status);
        Assert.NotNull(updated.ClosedAt);
    }

    // ── AddMessage ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddMessage_Fails_WhenContentIsEmpty()
    {
        var result = await _sut.AddMessage("user1", 1, "  ");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddMessage_Fails_WhenUserIdIsEmpty()
    {
        var result = await _sut.AddMessage("", 1, "hello");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddMessage_Succeeds_AndPersistsMessage()
    {
        var d = await SeedDiscussion();
        var result = await _sut.AddMessage("user1", d.Id, "Hello!");

        Assert.True(result.IsSuccess);
        var messages = _db.DiscussionMessages.Where(m => m.DiscussionId == d.Id).ToList();
        Assert.Single(messages);
        Assert.Equal("Hello!", messages[0].Content);
    }

    // ── SoftDeleteDiscussion ───────────────────────────────────────────────────

    [Fact]
    public async Task SoftDeleteDiscussion_Fails_WhenNotFound()
    {
        var result = await _sut.SoftDeleteDiscussion(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SoftDeleteDiscussion_Succeeds_AndMarksCascadeToMessages()
    {
        var d = await SeedDiscussion();
        _db.DiscussionMessages.AddRange(
            new DiscussionMessage { DiscussionId = d.Id, AuthorId = "u1", Content = "msg1" },
            new DiscussionMessage { DiscussionId = d.Id, AuthorId = "u1", Content = "msg2" });
        await _db.SaveChangesAsync();

        var result = await _sut.SoftDeleteDiscussion(d.Id);

        Assert.True(result.IsSuccess);
        var discussion = await _db.Discussions.IgnoreQueryFilters().FirstAsync(x => x.Id == d.Id);
        Assert.True(discussion.IsDeleted);
        var messages = _db.DiscussionMessages.IgnoreQueryFilters()
            .Where(m => m.DiscussionId == d.Id).ToList();
        Assert.All(messages, m => Assert.True(m.IsDeleted));
    }

    // ── AutoResolveDiscussionsByTicketId ───────────────────────────────────────

    [Fact]
    public async Task AutoResolve_ReturnsSuccess_WhenNoOpenDiscussions()
    {
        var result = await _sut.AutoResolveDiscussionsByTicketId(999);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AutoResolve_ResolvesAllOpenDiscussionsForTicket()
    {
        var d1 = await SeedDiscussion();
        var d2 = await SeedDiscussion();
        d1.TicketId = 42;
        d2.TicketId = 42;
        await _db.SaveChangesAsync();

        var result = await _sut.AutoResolveDiscussionsByTicketId(42);

        Assert.True(result.IsSuccess);
        var discussions = _db.Discussions.Where(d => d.TicketId == 42).ToList();
        Assert.All(discussions, d => Assert.Equal(Status.Resolved, d.Status));
    }

    [Fact]
    public async Task AutoResolve_DoesNotAffectDiscussionsOfOtherTickets()
    {
        var d = await SeedDiscussion();
        d.TicketId = 10;
        await _db.SaveChangesAsync();

        await _sut.AutoResolveDiscussionsByTicketId(99);

        var unchanged = await _db.Discussions.FirstAsync(x => x.Id == d.Id);
        Assert.Equal(Status.Open, unchanged.Status);
    }
}
