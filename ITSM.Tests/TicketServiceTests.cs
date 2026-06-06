using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Ticket;
using ITSM.Services.TicketCategory;
using ITSM.Services.Automation;
using ITSM.Services.Discussion;
using ITSM.Services.KnowledgeBase;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Tests;

public class TicketServiceTests
{
    private const string DefaultAuthorId = "seed-author";

    private readonly DBaseContext _db;
    private readonly TicketService _sut;
    private readonly Mock<IAutoServiceService> _autoService;
    private readonly Mock<IDiscussionService> _discussionService;
    private readonly Mock<ITicketCategoryService> _categoryService;
    private readonly Mock<IKnowledgeBaseService> _knowledgeBase;

    public TicketServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        // Ensure a default author exists so FK is satisfied on Include(Author)
        _db.Users.Add(new User { Id = DefaultAuthorId, UserName = "seed", Email = "seed@test.com" });
        _db.SaveChanges();

        _autoService = new Mock<IAutoServiceService>();
        _autoService
            .Setup(a => a.AssignTicketToAvailableUserAsync(It.IsAny<int>()))
            .ReturnsAsync(OperationResult.Success("ok"));

        _discussionService = new Mock<IDiscussionService>();
        _discussionService
            .Setup(d => d.AutoResolveDiscussionsByTicketId(It.IsAny<int>()))
            .ReturnsAsync(OperationResult.Success("ok"));

        _categoryService = new Mock<ITicketCategoryService>();
        _categoryService.Setup(c => c.GetAllCategoriesToList()).ReturnsAsync(new List<TicketCategory>());
        _categoryService.Setup(c => c.GetSubCategoriesForCategory(It.IsAny<int?>())).ReturnsAsync(new List<TicketSubCategory>());
        _categoryService.Setup(c => c.GetCategorySelectListAsync(It.IsAny<List<TicketCategory>?>())).ReturnsAsync(new List<SelectListItem>());
        _categoryService.Setup(c => c.MapSubCategoriesToSelectList(It.IsAny<List<TicketSubCategory>>())).Returns(new List<SelectListItem>());

        _knowledgeBase = new Mock<IKnowledgeBaseService>();
        _knowledgeBase.Setup(k => k.GetArticlesForSelect()).ReturnsAsync(new List<SelectListItem>());

        _sut = new TicketService(_db, _categoryService.Object, _autoService.Object, _discussionService.Object, _knowledgeBase.Object);
    }

    private async Task<Ticket> SeedTicket(Status status = Status.New, string? assignedUserId = null, string? authorId = null)
    {
        var ticket = new Ticket
        {
            Title = "Test Ticket",
            Description = "Description",
            Status = status,
            AssignedUserId = assignedUserId,
            AuthorId = authorId ?? DefaultAuthorId,
            CategoryId = 1
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    private async Task EnsureUser(string userId)
    {
        if (!await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == userId))
        {
            _db.Users.Add(new User { Id = userId, UserName = userId, Email = $"{userId}@test.com" });
            await _db.SaveChangesAsync();
        }
    }

    // ── GetTicketById ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTicketById_ReturnsTicket_WhenExists()
    {
        var ticket = await SeedTicket();
        var result = await _sut.GetTicketById(ticket.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ticket.Id);
    }

    [Fact]
    public async Task GetTicketById_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetTicketById(999);
        result.Should().BeNull();
    }

    // ── CreateNewTicket ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateNewTicket_Fails_WhenTitleIsEmpty()
    {
        var model = new TicketCreateViewModel { Title = "", Description = "Desc", CategoryId = 1 };
        var result = await _sut.CreateNewTicket(model, "user1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateNewTicket_Fails_WhenDescriptionIsWhitespace()
    {
        var model = new TicketCreateViewModel { Title = "Title", Description = "   ", CategoryId = 1 };
        var result = await _sut.CreateNewTicket(model, "user1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateNewTicket_Fails_WhenSubCategoryDoesNotBelongToCategory()
    {
        var sub = new TicketSubCategory { Name = "Sub", CategoryId = 1 };
        _db.TicketSubCategories.Add(sub);
        await _db.SaveChangesAsync();

        var model = new TicketCreateViewModel { Title = "T", Description = "D", CategoryId = 2, SubCategoryId = sub.Id };
        var result = await _sut.CreateNewTicket(model, "user1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateNewTicket_Succeeds_AndTriggersAutoAssign()
    {
        var model = new TicketCreateViewModel { Title = "T", Description = "D", CategoryId = 1 };
        var result = await _sut.CreateNewTicket(model, "user1");

        result.IsSuccess.Should().BeTrue();
        _autoService.Verify(a => a.AssignTicketToAvailableUserAsync(It.IsAny<int>()), Times.Once);
        _db.Tickets.Should().HaveCount(1);
    }

    // ── ResolveTicket ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveTicket_Fails_WhenNotFound()
    {
        var result = await _sut.ResolveTicket(999, "fix", null);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveTicket_Fails_WhenNotInProgress()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.ResolveTicket(ticket.Id, "fix", null);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveTicket_Succeeds_AndSetsStatusAndClosedAt()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.ResolveTicket(ticket.Id, "fixed it", null);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.Status.Should().Be(Status.Resolved);
        updated.ClosedAt.Should().NotBeNull();
        updated.FixDescription.Should().Be("fixed it");
    }

    [Fact]
    public async Task ResolveTicket_Succeeds_AndAutoResolvesDiscussions()
    {
        var ticket = await SeedTicket(Status.Progress);
        await _sut.ResolveTicket(ticket.Id, "done", null);

        _discussionService.Verify(d => d.AutoResolveDiscussionsByTicketId(ticket.Id), Times.Once);
    }

    // ── CancelTicketAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task CancelTicket_Fails_WhenReasonIsEmpty()
    {
        var result = await _sut.CancelTicketAsync(1, "", "actor");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CancelTicket_Fails_WhenNotFound()
    {
        var result = await _sut.CancelTicketAsync(999, "reason", "actor");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CancelTicket_Succeeds_AndSetsStatusAndHistory()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.CancelTicketAsync(ticket.Id, "not needed", "actor1");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.Status.Should().Be(Status.Canceled);
        updated.ClosedAt.Should().NotBeNull();
        var history = _db.TicketHistory.Where(h => h.TicketId == ticket.Id).ToList();
        history.Should().ContainSingle();
    }

    // ── AcceptTicketProcessingAsync ────────────────────────────────────────────

    [Fact]
    public async Task AcceptTicket_Fails_WhenUserIdIsEmpty()
    {
        var result = await _sut.AcceptTicketProcessingAsync(1, "");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AcceptTicket_Fails_WhenTicketNotFound()
    {
        var result = await _sut.AcceptTicketProcessingAsync(999, "tech1");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AcceptTicket_Fails_WhenAssignedToAnotherTechnician()
    {
        var ticket = await SeedTicket(Status.Progress, "other-tech");
        var result = await _sut.AcceptTicketProcessingAsync(ticket.Id, "my-tech");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AcceptTicket_Succeeds_WhenUnassigned()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.AcceptTicketProcessingAsync(ticket.Id, "tech1");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.AssignedUserId.Should().Be("tech1");
        updated.Status.Should().Be(Status.Progress);
    }

    [Fact]
    public async Task AcceptTicket_Succeeds_WhenAlreadyAssignedToSameTechnician()
    {
        var ticket = await SeedTicket(Status.Progress, "tech1");
        var result = await _sut.AcceptTicketProcessingAsync(ticket.Id, "tech1");
        result.IsSuccess.Should().BeTrue();
    }

    // ── PlaceOnHoldAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task PlaceOnHold_Fails_WhenNotFound()
    {
        var result = await _sut.PlaceOnHoldAsync(999, "reason");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PlaceOnHold_Fails_WhenNotInProgress()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.PlaceOnHoldAsync(ticket.Id, "reason");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PlaceOnHold_Succeeds_AndSetsOnHoldStatus()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.PlaceOnHoldAsync(ticket.Id, "waiting for info");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.Status.Should().Be(Status.OnHold);
    }

    // ── ResumeProgressAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ResumeProgress_Fails_WhenNotFound()
    {
        var result = await _sut.ResumeProgressAsync(999);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResumeProgress_Fails_WhenNotOnHold()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.ResumeProgressAsync(ticket.Id);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResumeProgress_Succeeds_AndRestoresProgressStatus()
    {
        var ticket = await SeedTicket(Status.OnHold);
        var result = await _sut.ResumeProgressAsync(ticket.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.Status.Should().Be(Status.Progress);
    }

    // ── ReopenTicketAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ReopenTicket_Fails_WhenNotFound()
    {
        var result = await _sut.ReopenTicketAsync(999, "reason");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ReopenTicket_Fails_WhenNotResolved()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.ReopenTicketAsync(ticket.Id, "still broken");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ReopenTicket_Succeeds_AndClearsClosedAt()
    {
        var ticket = await SeedTicket(Status.Resolved);
        ticket.ClosedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var result = await _sut.ReopenTicketAsync(ticket.Id, "still broken");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.Status.Should().Be(Status.Reopened);
        updated.ClosedAt.Should().BeNull();
    }

    // ── UnassignTicketAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UnassignTicket_Fails_WhenNotFound()
    {
        var result = await _sut.UnassignTicketAsync(999);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UnassignTicket_Fails_WhenTicketNotAssigned()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.UnassignTicketAsync(ticket.Id);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UnassignTicket_Succeeds_AndClearsAssignmentAndSetsOpen()
    {
        var ticket = await SeedTicket(Status.Progress, "tech1");
        var result = await _sut.UnassignTicketAsync(ticket.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        updated!.AssignedUserId.Should().BeNull();
        updated.Status.Should().Be(Status.Open);
    }

    // ── ArchiveTicketAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveTicket_Fails_WhenNotFound()
    {
        var result = await _sut.ArchiveTicketAsync(999, "actor");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ArchiveTicket_Fails_WhenAlreadyArchived()
    {
        var ticket = await SeedTicket();
        ticket.IsDeleted = true;
        await _db.SaveChangesAsync();

        var result = await _sut.ArchiveTicketAsync(ticket.Id, "actor");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ArchiveTicket_Succeeds_AndMarksDeleted()
    {
        var ticket = await SeedTicket();
        var result = await _sut.ArchiveTicketAsync(ticket.Id, "actor");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == ticket.Id);
        updated!.IsDeleted.Should().BeTrue();
    }

    // ── RestoreTicketAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RestoreTicket_Fails_WhenNotFound()
    {
        var result = await _sut.RestoreTicketAsync(999, "actor");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreTicket_Fails_WhenNotArchived()
    {
        var ticket = await SeedTicket();
        var result = await _sut.RestoreTicketAsync(ticket.Id, "actor");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreTicket_Succeeds_AndUndeletion()
    {
        var ticket = await SeedTicket();
        ticket.IsDeleted = true;
        await _db.SaveChangesAsync();

        var result = await _sut.RestoreTicketAsync(ticket.Id, "actor");

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == ticket.Id);
        updated!.IsDeleted.Should().BeFalse();
    }

    // ── AddTicketStepAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task AddTicketStep_PersistsHistoryRecord()
    {
        var ticket = await SeedTicket();
        await _sut.AddTicketStepAsync(ticket.Id, "Step complete");

        var history = _db.TicketHistory.Where(h => h.TicketId == ticket.Id).ToList();
        history.Should().ContainSingle();
        history[0].AdminComment.Should().Be("Step complete");
    }

    // ── GetAllTickets ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllTickets_ReturnsAllNonDeletedTickets()
    {
        await SeedTicket();
        await SeedTicket();

        var result = await _sut.GetAllTickets();

        result.Should().HaveCount(2);
    }

    // ── GetAllTicketsQuery ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllTicketsQuery_ReturnsQueryable()
    {
        await SeedTicket(Status.New);
        await SeedTicket(Status.Progress);

        var result = _sut.GetAllTicketsQuery().ToList();

        result.Should().HaveCount(2);
    }

    // ── GetUserTickets ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserTickets_ReturnsOnlyTicketsForGivenAuthor()
    {
        await EnsureUser("user1");
        await EnsureUser("user2");
        await SeedTicket(authorId: "user1");
        await SeedTicket(authorId: "user2");

        var result = await _sut.GetUserTickets("user1");

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.AuthorId == "user1");
    }

    // ── GetUserTicketsQuery ────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserTicketsQuery_ReturnsQueryableFilteredByAuthor()
    {
        await EnsureUser("user1");
        await EnsureUser("user2");
        await SeedTicket(authorId: "user1");
        await SeedTicket(authorId: "user2");

        var result = _sut.GetUserTicketsQuery("user1").ToList();

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.AuthorId == "user1");
    }

    // ── GetTicketsAssignedToAdminAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetTicketsAssignedToAdminAsync_ReturnsOnlyAssignedTickets()
    {
        await SeedTicket(assignedUserId: "admin1");
        await SeedTicket(assignedUserId: "other");

        var result = await _sut.GetTicketsAssignedToAdminAsync("admin1");

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.AssignedUserId == "admin1");
    }

    // ── ChangeTicketStatus ─────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeTicketStatus_ReturnsFailure_WhenTicketNotFound()
    {
        var result = await _sut.ChangeTicketStatus(999, Status.Progress);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeTicketStatus_UpdatesStatus()
    {
        var ticket = await SeedTicket(Status.New);

        var result = await _sut.ChangeTicketStatus(ticket.Id, Status.Progress);

        result.IsSuccess.Should().BeTrue();
        _db.Tickets.Find(ticket.Id)!.Status.Should().Be(Status.Progress);
    }

    [Fact]
    public async Task ChangeTicketStatus_AutoResolvesDiscussions_WhenStatusIsResolved()
    {
        var ticket = await SeedTicket(Status.Progress);

        await _sut.ChangeTicketStatus(ticket.Id, Status.Resolved);

        _discussionService.Verify(d => d.AutoResolveDiscussionsByTicketId(ticket.Id), Times.Once);
    }

    // ── CreateTicketDetailsViewModel ───────────────────────────────────────────

    [Fact]
    public async Task CreateTicketDetailsViewModel_ReturnsViewModel_WithTicketData()
    {
        var ticket = await SeedTicket();

        var result = await _sut.CreateTicketDetailsViewModel(ticket);

        result.Ticket.Should().Be(ticket);
        result.TicketHistory.Should().NotBeNull();
        result.AvailableArticles.Should().NotBeNull();
    }

    // ── BuildCreateTicketViewModel ─────────────────────────────────────────────

    [Fact]
    public async Task BuildCreateTicketViewModel_ReturnsViewModel_WithSelectedCategoryId()
    {
        var result = await _sut.BuildCreateTicketViewModel(1);

        result.CategoryId.Should().Be(1);
    }
}
