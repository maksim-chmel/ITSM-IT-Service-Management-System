using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Automation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ITSM.Tests;

public class AutoServiceServiceTests
{
    private readonly DBaseContext _db;
    private readonly AutoServiceService _sut;
    private readonly Mock<UserManager<User>> _userManager;

    public AutoServiceServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new AutoServiceService(_db, _userManager.Object, Mock.Of<ILogger<AutoServiceService>>());
    }

    private async Task<User> SeedTechnicianWithCategory(int categoryId, SkillLevel skill = SkillLevel.Middle)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "tech",
            Email = "tech@test.com",
            SkillLevel = skill,
            IsDeleted = false
        };
        _db.Users.Add(user);
        _db.UserCategoryAssignments.Add(new UserCategoryAssignment
        {
            UserId = user.Id,
            CategoryId = categoryId
        });
        await _db.SaveChangesAsync();

        var allTechs = _db.Users.Where(u => !u.IsDeleted).ToList();
        _userManager
            .Setup(m => m.GetUsersInRoleAsync(nameof(UserRoles.Technician)))
            .ReturnsAsync(allTechs);

        return user;
    }

    private async Task<Ticket> SeedTicket(int categoryId, Status status = Status.New, TicketPriority priority = TicketPriority.Medium)
    {
        var ticket = new Ticket
        {
            Title = "T",
            Description = "D",
            CategoryId = categoryId,
            Status = status,
            Priority = priority
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    // ── AssignTicketToAvailableUserAsync ───────────────────────────────────────

    [Fact]
    public async Task AssignTicketToAvailableUserAsync_ReturnsFailure_WhenTicketNotFound()
    {
        var result = await _sut.AssignTicketToAvailableUserAsync(999);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AssignTicketToAvailableUserAsync_ReturnsFailure_WhenNoMatchingTechnician()
    {
        var ticket = await SeedTicket(categoryId: 1);
        _userManager.Setup(m => m.GetUsersInRoleAsync(nameof(UserRoles.Technician)))
                   .ReturnsAsync(new List<User>());

        var result = await _sut.AssignTicketToAvailableUserAsync(ticket.Id);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AssignTicketToAvailableUserAsync_AssignsTicket_WhenTechnicianMatchesCategory()
    {
        var tech = await SeedTechnicianWithCategory(categoryId: 1);
        var ticket = await SeedTicket(categoryId: 1);

        var result = await _sut.AssignTicketToAvailableUserAsync(ticket.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Tickets.IgnoreQueryFilters().FirstAsync(t => t.Id == ticket.Id);
        updated.AssignedUserId.Should().Be(tech.Id);
        updated.Status.Should().Be(Status.Progress);
    }

    // ── AssignTicketsByCategoryAndLoadAsync ────────────────────────────────────

    [Fact]
    public async Task AssignTicketsByCategoryAndLoadAsync_AssignsNewTickets_ToMatchingTechnician()
    {
        var tech = await SeedTechnicianWithCategory(categoryId: 1);
        var ticket = await SeedTicket(categoryId: 1, status: Status.New);

        await _sut.AssignTicketsByCategoryAndLoadAsync();

        var updated = await _db.Tickets.IgnoreQueryFilters().FirstAsync(t => t.Id == ticket.Id);
        updated.AssignedUserId.Should().Be(tech.Id);
        updated.Status.Should().Be(Status.Progress);
    }

    [Fact]
    public async Task AssignTicketsByCategoryAndLoadAsync_DoesNotAssign_WhenNoMatchingTechnician()
    {
        _userManager.Setup(m => m.GetUsersInRoleAsync(nameof(UserRoles.Technician)))
                   .ReturnsAsync(new List<User>());
        var ticket = await SeedTicket(categoryId: 1, status: Status.New);

        await _sut.AssignTicketsByCategoryAndLoadAsync();

        var updated = await _db.Tickets.IgnoreQueryFilters().FirstAsync(t => t.Id == ticket.Id);
        updated.AssignedUserId.Should().BeNull();
    }

    // ── ResetTicketsAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResetTicketsAsync_ResetsNonFinalTickets_ToNewWithNoAssignee()
    {
        var tech = await SeedTechnicianWithCategory(categoryId: 1);
        var ticket = await SeedTicket(categoryId: 1, status: Status.Progress);
        ticket.AssignedUserId = tech.Id;
        await _db.SaveChangesAsync();

        await _sut.ResetTicketsAsync();

        var updated = _db.Tickets.Find(ticket.Id)!;
        updated.Status.Should().Be(Status.New);
        updated.AssignedUserId.Should().BeNull();
    }

    [Fact]
    public async Task ResetTicketsAsync_DoesNotAffect_ResolvedAndCanceledTickets()
    {
        var resolved = await SeedTicket(categoryId: 1, status: Status.Resolved);
        var canceled = await SeedTicket(categoryId: 1, status: Status.Canceled);

        await _sut.ResetTicketsAsync();

        _db.Tickets.Find(resolved.Id)!.Status.Should().Be(Status.Resolved);
        _db.Tickets.Find(canceled.Id)!.Status.Should().Be(Status.Canceled);
    }
}
