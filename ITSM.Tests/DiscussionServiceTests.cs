using FluentAssertions;
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

    private async Task<User> EnsureUser(string userId)
    {
        var existing = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        if (existing != null) return existing;
        var user = new User { Id = userId, UserName = userId, Email = $"{userId}@test.com" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private async Task<Discussion> SeedDiscussion(string authorId = "user1", Status status = Status.Open)
    {
        await EnsureUser(authorId);
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
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateDiscussion_Fails_WhenDescriptionIsWhitespace()
    {
        var model = new DiscussionCreateViewModel { Title = "Title", Description = "  ", CategoryId = 1 };
        var result = await _sut.CreateDiscussion(model, "user1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateDiscussion_Succeeds_AndPersistsRecord()
    {
        var model = new DiscussionCreateViewModel { Title = "My Topic", Description = "Details", CategoryId = 1 };
        var result = await _sut.CreateDiscussion(model, "user1");

        result.IsSuccess.Should().BeTrue();
        _db.Discussions.Should().HaveCount(1);
    }

    // ── ResolveDiscussion ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveDiscussion_Fails_WhenNotFound()
    {
        var result = await _sut.ResolveDiscussion(999, "user1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDiscussion_Fails_WhenCallerIsNotAuthor()
    {
        var d = await SeedDiscussion("author1");
        var result = await _sut.ResolveDiscussion(d.Id, "someone-else");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDiscussion_Succeeds_AndSetsResolvedStatusAndClosedAt()
    {
        var d = await SeedDiscussion("author1");
        var result = await _sut.ResolveDiscussion(d.Id, "author1");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Discussions.FirstAsync(x => x.Id == d.Id);
        updated.Status.Should().Be(Status.Resolved);
        updated.ClosedAt.Should().NotBeNull();
    }

    // ── GetAllDiscussions ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllDiscussions_ReturnsOnlyMatchingStatus()
    {
        await SeedDiscussion(status: Status.Open);
        await SeedDiscussion(status: Status.Resolved);

        var result = await _sut.GetAllDiscussions(Status.Open);

        result.Should().HaveCount(1)
              .And.OnlyContain(d => d.Status == Status.Open);
    }

    [Fact]
    public async Task GetAllDiscussions_ReturnsEmpty_WhenNoMatchingStatus()
    {
        await SeedDiscussion(status: Status.Open);

        var result = await _sut.GetAllDiscussions(Status.Resolved);

        result.Should().BeEmpty();
    }

    // ── GetDiscussionByIdWithMessages ──────────────────────────────────────────

    [Fact]
    public async Task GetDiscussionByIdWithMessages_ReturnsDiscussion_WhenFound()
    {
        var d = await SeedDiscussion();

        var result = await _sut.GetDiscussionByIdWithMessages(d.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(d.Id);
    }

    [Fact]
    public async Task GetDiscussionByIdWithMessages_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetDiscussionByIdWithMessages(999);
        result.Should().BeNull();
    }

    // ── GetUserDiscussions ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserDiscussions_ReturnsOnlyUserDiscussions()
    {
        await SeedDiscussion(authorId: "user1");
        await SeedDiscussion(authorId: "user2");

        var result = await _sut.GetUserDiscussions("user1");

        result.Should().HaveCount(1)
              .And.OnlyContain(d => d.AuthorId == "user1");
    }

    [Fact]
    public async Task GetUserDiscussions_ReturnsEmpty_WhenUserHasNone()
    {
        await SeedDiscussion(authorId: "other");

        var result = await _sut.GetUserDiscussions("user1");

        result.Should().BeEmpty();
    }

    // ── SearchDiscussion ───────────────────────────────────────────────────────

    [Fact]
    public async Task SearchDiscussion_ReturnsMatchingTitleAndStatus()
    {
        await EnsureUser("u1");
        _db.Discussions.Add(new Discussion
        {
            Title = "Alpha Topic", Description = "D",
            AuthorId = "u1", CategoryId = 1, Status = Status.Open
        });
        await _db.SaveChangesAsync();
        await SeedDiscussion(status: Status.Open); // title = "Topic"

        var result = await _sut.SearchDiscussion(Status.Open, "alpha");

        result.Should().HaveCount(1)
              .And.Contain(d => d.Title == "Alpha Topic");
    }

    [Fact]
    public async Task SearchDiscussion_ReturnsEmpty_WhenStatusDoesNotMatch()
    {
        await SeedDiscussion(status: Status.Open);

        var result = await _sut.SearchDiscussion(Status.Resolved, "Topic");

        result.Should().BeEmpty();
    }

    // ── AddMessage ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddMessage_Fails_WhenContentIsEmpty()
    {
        var result = await _sut.AddMessage("user1", 1, "  ");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddMessage_Fails_WhenUserIdIsEmpty()
    {
        var result = await _sut.AddMessage("", 1, "hello");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddMessage_Succeeds_AndPersistsMessage()
    {
        var d = await SeedDiscussion();
        var result = await _sut.AddMessage("user1", d.Id, "Hello!");

        result.IsSuccess.Should().BeTrue();
        var messages = _db.DiscussionMessages.Where(m => m.DiscussionId == d.Id).ToList();
        messages.Should().ContainSingle();
        messages[0].Content.Should().Be("Hello!");
    }

    // ── SoftDeleteDiscussion ───────────────────────────────────────────────────

    [Fact]
    public async Task SoftDeleteDiscussion_Fails_WhenNotFound()
    {
        var result = await _sut.SoftDeleteDiscussion(999);
        result.IsSuccess.Should().BeFalse();
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

        result.IsSuccess.Should().BeTrue();
        var discussion = await _db.Discussions.IgnoreQueryFilters().FirstAsync(x => x.Id == d.Id);
        discussion.IsDeleted.Should().BeTrue();
        var messages = _db.DiscussionMessages.IgnoreQueryFilters()
            .Where(m => m.DiscussionId == d.Id).ToList();
        messages.Should().OnlyContain(m => m.IsDeleted);
    }

    // ── RestoreDiscussion ──────────────────────────────────────────────────────

    [Fact]
    public async Task RestoreDiscussion_Fails_WhenNotFound()
    {
        var result = await _sut.RestoreDiscussion(999);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreDiscussion_Fails_WhenNotArchived()
    {
        var d = await SeedDiscussion();

        var result = await _sut.RestoreDiscussion(d.Id);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreDiscussion_Succeeds_AndRestoresDiscussionAndMessages()
    {
        var d = await SeedDiscussion();
        _db.DiscussionMessages.Add(new DiscussionMessage
        {
            DiscussionId = d.Id, AuthorId = "u1", Content = "msg"
        });
        await _db.SaveChangesAsync();

        await _sut.SoftDeleteDiscussion(d.Id);
        _db.ChangeTracker.Clear();

        var result = await _sut.RestoreDiscussion(d.Id);

        result.IsSuccess.Should().BeTrue();
        var restored = await _db.Discussions.IgnoreQueryFilters().FirstAsync(x => x.Id == d.Id);
        restored.IsDeleted.Should().BeFalse();
        var msgs = _db.DiscussionMessages.IgnoreQueryFilters()
            .Where(m => m.DiscussionId == d.Id).ToList();
        msgs.Should().OnlyContain(m => !m.IsDeleted);
    }

    // ── AutoResolveDiscussionsByTicketId ───────────────────────────────────────

    [Fact]
    public async Task AutoResolve_ReturnsSuccess_WhenNoOpenDiscussions()
    {
        var result = await _sut.AutoResolveDiscussionsByTicketId(999);
        result.IsSuccess.Should().BeTrue();
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

        result.IsSuccess.Should().BeTrue();
        var discussions = _db.Discussions.Where(d => d.TicketId == 42).ToList();
        discussions.Should().OnlyContain(d => d.Status == Status.Resolved);
    }

    [Fact]
    public async Task AutoResolve_DoesNotAffectDiscussionsOfOtherTickets()
    {
        var d = await SeedDiscussion();
        d.TicketId = 10;
        await _db.SaveChangesAsync();

        await _sut.AutoResolveDiscussionsByTicketId(99);

        var unchanged = await _db.Discussions.FirstAsync(x => x.Id == d.Id);
        unchanged.Status.Should().Be(Status.Open);
    }
}
