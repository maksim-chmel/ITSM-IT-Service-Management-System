using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.TicketSort;

namespace ITSM.Tests;

public class TicketSortServiceTests
{
    private readonly DBaseContext _db;
    private readonly TicketSortService _sut;

    public TicketSortServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        _sut = new TicketSortService(_db);
    }

    private async Task<TicketCategory> SeedCategory(string name = "IT")
    {
        var cat = new TicketCategory { Name = name };
        _db.TicketCategories.Add(cat);
        await _db.SaveChangesAsync();
        return cat;
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

    // ── GetFilteredTickets ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetFilteredTickets_ReturnsAll_WhenNoFilters()
    {
        var cat = await SeedCategory();
        await SeedTicket(cat.Id, Status.New, TicketPriority.Low);
        await SeedTicket(cat.Id, Status.Progress, TicketPriority.High);

        var result = _sut.GetFilteredTickets(_db.Tickets, null, null, null).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilteredTickets_FiltersByCategory()
    {
        var cat1 = await SeedCategory("A");
        var cat2 = await SeedCategory("B");
        await SeedTicket(cat1.Id);
        await SeedTicket(cat2.Id);

        var result = _sut.GetFilteredTickets(_db.Tickets, cat1.Id, null, null).ToList();

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.CategoryId == cat1.Id);
    }

    [Fact]
    public async Task GetFilteredTickets_FiltersByPriority()
    {
        var cat = await SeedCategory();
        await SeedTicket(cat.Id, priority: TicketPriority.High);
        await SeedTicket(cat.Id, priority: TicketPriority.Low);

        var result = _sut.GetFilteredTickets(_db.Tickets, null, TicketPriority.High, null).ToList();

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.Priority == TicketPriority.High);
    }

    [Fact]
    public async Task GetFilteredTickets_FiltersByStatus()
    {
        var cat = await SeedCategory();
        await SeedTicket(cat.Id, status: Status.New);
        await SeedTicket(cat.Id, status: Status.Resolved);

        var result = _sut.GetFilteredTickets(_db.Tickets, null, null, Status.New).ToList();

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.Status == Status.New);
    }

    [Fact]
    public async Task GetFilteredTickets_CombinesMultipleFilters()
    {
        var cat1 = await SeedCategory("X");
        var cat2 = await SeedCategory("Y");
        await SeedTicket(cat1.Id, Status.New, TicketPriority.High);
        await SeedTicket(cat1.Id, Status.Progress, TicketPriority.High);
        await SeedTicket(cat2.Id, Status.New, TicketPriority.High);

        var result = _sut.GetFilteredTickets(_db.Tickets, cat1.Id, TicketPriority.High, Status.New).ToList();

        result.Should().HaveCount(1)
              .And.OnlyContain(t => t.CategoryId == cat1.Id && t.Status == Status.New);
    }

    // ── GetCategorySelectList ──────────────────────────────────────────────────

    [Fact]
    public void GetCategorySelectList_IncludesSeededCategories()
    {
        // EnsureCreated seeds 5 default categories
        var result = _sut.GetCategorySelectList().ToList();

        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCategorySelectList_IncludesNewlyAddedCategory()
    {
        await SeedCategory("Custom Support");

        var result = _sut.GetCategorySelectList().ToList();

        result.Should().Contain(item => item.Text == "Custom Support");
    }
}
