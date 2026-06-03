using Moq;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Ticket;
using ITSM.Services.TicketCategory;
using ITSM.Services.Automation;
using ITSM.Services.Discussion;
using ITSM.ViewModels.Create;

namespace ITSM.Tests;

public class TicketServiceTests
{
    private readonly DBaseContext _db;
    private readonly TicketService _sut;
    private readonly Mock<IAutoServiceService> _autoService;
    private readonly Mock<IDiscussionService> _discussionService;

    public TicketServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        _autoService = new Mock<IAutoServiceService>();
        _autoService
            .Setup(a => a.AssignTicketToAvailableUserAsync(It.IsAny<int>()))
            .ReturnsAsync(OperationResult.Success("ok"));

        _discussionService = new Mock<IDiscussionService>();
        _discussionService
            .Setup(d => d.AutoResolveDiscussionsByTicketId(It.IsAny<int>()))
            .ReturnsAsync(OperationResult.Success("ok"));

        _sut = new TicketService(_db, new Mock<ITicketCategoryService>().Object,
            _autoService.Object, _discussionService.Object);
    }

    private async Task<Ticket> SeedTicket(Status status = Status.New, string? assignedUserId = null)
    {
        var ticket = new Ticket
        {
            Title = "Test Ticket",
            Description = "Description",
            Status = status,
            AssignedUserId = assignedUserId
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    // ── GetTicketById ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTicketById_ReturnsTicket_WhenExists()
    {
        var ticket = await SeedTicket();
        var result = await _sut.GetTicketById(ticket.Id);
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
    }

    [Fact]
    public async Task GetTicketById_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetTicketById(999);
        Assert.Null(result);
    }

    // ── CreateNewTicket ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateNewTicket_Fails_WhenTitleIsEmpty()
    {
        var model = new TicketCreateViewModel { Title = "", Description = "Desc", CategoryId = 1 };
        var result = await _sut.CreateNewTicket(model, "user1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateNewTicket_Fails_WhenDescriptionIsWhitespace()
    {
        var model = new TicketCreateViewModel { Title = "Title", Description = "   ", CategoryId = 1 };
        var result = await _sut.CreateNewTicket(model, "user1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateNewTicket_Fails_WhenSubCategoryDoesNotBelongToCategory()
    {
        var sub = new TicketSubCategory { Name = "Sub", CategoryId = 1 };
        _db.TicketSubCategories.Add(sub);
        await _db.SaveChangesAsync();

        var model = new TicketCreateViewModel { Title = "T", Description = "D", CategoryId = 2, SubCategoryId = sub.Id };
        var result = await _sut.CreateNewTicket(model, "user1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateNewTicket_Succeeds_AndTriggersAutoAssign()
    {
        var model = new TicketCreateViewModel { Title = "T", Description = "D", CategoryId = 1 };
        var result = await _sut.CreateNewTicket(model, "user1");

        Assert.True(result.IsSuccess);
        _autoService.Verify(a => a.AssignTicketToAvailableUserAsync(It.IsAny<int>()), Times.Once);
        Assert.Equal(1, _db.Tickets.Count());
    }

    // ── ResolveTicket ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveTicket_Fails_WhenNotFound()
    {
        var result = await _sut.ResolveTicket(999, "fix", null);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResolveTicket_Fails_WhenNotInProgress()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.ResolveTicket(ticket.Id, "fix", null);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResolveTicket_Succeeds_AndSetsStatusAndClosedAt()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.ResolveTicket(ticket.Id, "fixed it", null);

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(Status.Resolved, updated!.Status);
        Assert.NotNull(updated.ClosedAt);
        Assert.Equal("fixed it", updated.FixDescription);
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
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CancelTicket_Fails_WhenNotFound()
    {
        var result = await _sut.CancelTicketAsync(999, "reason", "actor");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CancelTicket_Succeeds_AndSetsStatusAndHistory()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.CancelTicketAsync(ticket.Id, "not needed", "actor1");

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(Status.Canceled, updated!.Status);
        Assert.NotNull(updated.ClosedAt);
        var history = _db.TicketHistory.Where(h => h.TicketId == ticket.Id).ToList();
        Assert.Single(history);
    }

    // ── AcceptTicketProcessingAsync ────────────────────────────────────────────

    [Fact]
    public async Task AcceptTicket_Fails_WhenUserIdIsEmpty()
    {
        var result = await _sut.AcceptTicketProcessingAsync(1, "");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AcceptTicket_Fails_WhenTicketNotFound()
    {
        var result = await _sut.AcceptTicketProcessingAsync(999, "tech1");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AcceptTicket_Fails_WhenAssignedToAnotherTechnician()
    {
        var ticket = await SeedTicket(Status.Progress, "other-tech");
        var result = await _sut.AcceptTicketProcessingAsync(ticket.Id, "my-tech");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AcceptTicket_Succeeds_WhenUnassigned()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.AcceptTicketProcessingAsync(ticket.Id, "tech1");

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Equal("tech1", updated!.AssignedUserId);
        Assert.Equal(Status.Progress, updated.Status);
    }

    [Fact]
    public async Task AcceptTicket_Succeeds_WhenAlreadyAssignedToSameTechnician()
    {
        var ticket = await SeedTicket(Status.Progress, "tech1");
        var result = await _sut.AcceptTicketProcessingAsync(ticket.Id, "tech1");
        Assert.True(result.IsSuccess);
    }

    // ── PlaceOnHoldAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task PlaceOnHold_Fails_WhenNotFound()
    {
        var result = await _sut.PlaceOnHoldAsync(999, "reason");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task PlaceOnHold_Fails_WhenNotInProgress()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.PlaceOnHoldAsync(ticket.Id, "reason");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task PlaceOnHold_Succeeds_AndSetsOnHoldStatus()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.PlaceOnHoldAsync(ticket.Id, "waiting for info");

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(Status.OnHold, updated!.Status);
    }

    // ── ResumeProgressAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ResumeProgress_Fails_WhenNotFound()
    {
        var result = await _sut.ResumeProgressAsync(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResumeProgress_Fails_WhenNotOnHold()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.ResumeProgressAsync(ticket.Id);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ResumeProgress_Succeeds_AndRestoresProgressStatus()
    {
        var ticket = await SeedTicket(Status.OnHold);
        var result = await _sut.ResumeProgressAsync(ticket.Id);

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(Status.Progress, updated!.Status);
    }

    // ── ReopenTicketAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ReopenTicket_Fails_WhenNotFound()
    {
        var result = await _sut.ReopenTicketAsync(999, "reason");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ReopenTicket_Fails_WhenNotResolved()
    {
        var ticket = await SeedTicket(Status.Progress);
        var result = await _sut.ReopenTicketAsync(ticket.Id, "still broken");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ReopenTicket_Succeeds_AndClearsClosedAt()
    {
        var ticket = await SeedTicket(Status.Resolved);
        ticket.ClosedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var result = await _sut.ReopenTicketAsync(ticket.Id, "still broken");

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(Status.Reopened, updated!.Status);
        Assert.Null(updated.ClosedAt);
    }

    // ── UnassignTicketAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UnassignTicket_Fails_WhenNotFound()
    {
        var result = await _sut.UnassignTicketAsync(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UnassignTicket_Fails_WhenTicketNotAssigned()
    {
        var ticket = await SeedTicket(Status.New);
        var result = await _sut.UnassignTicketAsync(ticket.Id);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UnassignTicket_Succeeds_AndClearsAssignmentAndSetsOpen()
    {
        var ticket = await SeedTicket(Status.Progress, "tech1");
        var result = await _sut.UnassignTicketAsync(ticket.Id);

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.FindAsync(ticket.Id);
        Assert.Null(updated!.AssignedUserId);
        Assert.Equal(Status.Open, updated.Status);
    }

    // ── ArchiveTicketAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveTicket_Fails_WhenNotFound()
    {
        var result = await _sut.ArchiveTicketAsync(999, "actor");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ArchiveTicket_Fails_WhenAlreadyArchived()
    {
        var ticket = await SeedTicket();
        ticket.IsDeleted = true;
        await _db.SaveChangesAsync();

        var result = await _sut.ArchiveTicketAsync(ticket.Id, "actor");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ArchiveTicket_Succeeds_AndMarksDeleted()
    {
        var ticket = await SeedTicket();
        var result = await _sut.ArchiveTicketAsync(ticket.Id, "actor");

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == ticket.Id);
        Assert.True(updated!.IsDeleted);
    }

    // ── RestoreTicketAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RestoreTicket_Fails_WhenNotFound()
    {
        var result = await _sut.RestoreTicketAsync(999, "actor");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RestoreTicket_Fails_WhenNotArchived()
    {
        var ticket = await SeedTicket();
        var result = await _sut.RestoreTicketAsync(ticket.Id, "actor");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RestoreTicket_Succeeds_AndUndeletion()
    {
        var ticket = await SeedTicket();
        ticket.IsDeleted = true;
        await _db.SaveChangesAsync();

        var result = await _sut.RestoreTicketAsync(ticket.Id, "actor");

        Assert.True(result.IsSuccess);
        var updated = await _db.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == ticket.Id);
        Assert.False(updated!.IsDeleted);
    }

    // ── AddTicketStepAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task AddTicketStep_PersistsHistoryRecord()
    {
        var ticket = await SeedTicket();
        await _sut.AddTicketStepAsync(ticket.Id, "Step complete");

        var history = _db.TicketHistory.Where(h => h.TicketId == ticket.Id).ToList();
        Assert.Single(history);
        Assert.Equal("Step complete", history[0].AdminComment);
    }
}
