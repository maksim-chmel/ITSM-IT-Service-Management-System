using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Ticket;
using ITSM.Services.TicketAssignment;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Tests;

public class TicketAssignmentServiceTests
{
    private readonly DBaseContext _db;
    private readonly TicketAssignmentService _sut;
    private readonly Mock<ITicketService> _ticketService;

    public TicketAssignmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        _ticketService = new Mock<ITicketService>();

        _sut = new TicketAssignmentService(_ticketService.Object, _db);
    }

    private async Task<Ticket> SeedTicket(int categoryId = 1, Status status = Status.New)
    {
        var ticket = new Ticket
        {
            Title = "T",
            Description = "D",
            CategoryId = categoryId,
            Status = status
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    private async Task<User> SeedTechnician(string userId = "tech-1", bool isDeleted = false)
    {
        var user = new User { Id = userId, UserName = "tech", Email = "tech@test.com", IsDeleted = isDeleted };
        _db.Users.Add(user);

        var role = new IdentityRole { Id = "role-tech", Name = nameof(UserRoles.Technician), NormalizedName = "TECHNICIAN" };
        if (!_db.Roles.Any(r => r.Id == role.Id))
            _db.Roles.Add(role);

        _db.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = "role-tech" });
        await _db.SaveChangesAsync();
        return user;
    }

    // ── AssignTicketToTechnician ───────────────────────────────────────────────

    [Fact]
    public async Task AssignTicketToTechnician_ReturnsFailure_WhenTicketNotFound()
    {
        var result = await _sut.AssignTicketToTechnician(999, "user-1");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AssignTicketToTechnician_ReturnsFailure_WhenTechnicianNotFound()
    {
        var ticket = await SeedTicket();

        var result = await _sut.AssignTicketToTechnician(ticket.Id, "nonexistent");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AssignTicketToTechnician_AssignsTicket_AndAddsHistory()
    {
        var tech = await SeedTechnician("tech-1");
        var ticket = await SeedTicket();

        var result = await _sut.AssignTicketToTechnician(ticket.Id, tech.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.IgnoreQueryFilters().FirstAsync(t => t.Id == ticket.Id);
        updated.AssignedUserId.Should().Be(tech.Id);
        updated.Status.Should().Be(Status.Progress);
        _db.TicketHistory.IgnoreQueryFilters().Should().Contain(h => h.TicketId == ticket.Id);
    }

    [Fact]
    public async Task AssignTicketToTechnician_ReturnsFailure_WhenUserIsDeletedTechnician()
    {
        var tech = await SeedTechnician("tech-deleted", isDeleted: true);
        var ticket = await SeedTicket();

        var result = await _sut.AssignTicketToTechnician(ticket.Id, tech.Id);

        result.IsSuccess.Should().BeFalse();
    }

    // ── UpdateTicketPriority ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTicketPriority_ReturnsFailure_WhenTicketNotFound()
    {
        _ticketService.Setup(m => m.GetTicketById(999)).ReturnsAsync((Ticket?)null);

        var result = await _sut.UpdateTicketPriority(999, TicketPriority.High);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTicketPriority_UpdatesPriority_AndReturnsSuccess()
    {
        var ticket = await SeedTicket();
        _ticketService.Setup(m => m.GetTicketById(ticket.Id)).ReturnsAsync(ticket);

        var result = await _sut.UpdateTicketPriority(ticket.Id, TicketPriority.Critical);

        result.IsSuccess.Should().BeTrue();
        ticket.Priority.Should().Be(TicketPriority.Critical);
    }

    // ── CreateAssignTechnicianViewModel ────────────────────────────────────────

    [Fact]
    public async Task CreateAssignTechnicianViewModel_Throws_WhenTicketNotFound()
    {
        Func<Task> act = () => _sut.CreateAssignTechnicianViewModel(999);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateAssignTechnicianViewModel_ReturnsTechniciansMatchingCategory()
    {
        var tech = await SeedTechnician("tech-cat");
        _db.UserCategoryAssignments.Add(new UserCategoryAssignment { UserId = tech.Id, CategoryId = 1 });
        var ticket = await SeedTicket(categoryId: 1);
        await _db.SaveChangesAsync();

        var result = await _sut.CreateAssignTechnicianViewModel(ticket.Id);

        result.Ticket.Id.Should().Be(ticket.Id);
        result.Users.Should().ContainSingle(u => u.Value == tech.Id);
    }

    // ── CreateAssignPriorityViewModel ──────────────────────────────────────────

    [Fact]
    public async Task CreateAssignPriorityViewModel_ReturnsAllPriorities()
    {
        var ticket = await SeedTicket();
        _ticketService.Setup(m => m.GetTicketById(ticket.Id)).ReturnsAsync(ticket);

        var result = await _sut.CreateAssignPriorityViewModel(ticket.Id);

        result.Ticket.Should().Be(ticket);
        result.Priorities.Should().HaveCount(Enum.GetValues(typeof(TicketPriority)).Length);
    }
}
