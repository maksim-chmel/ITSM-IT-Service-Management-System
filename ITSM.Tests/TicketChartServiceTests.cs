using FluentAssertions;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Charts;

namespace ITSM.Tests;

public class TicketChartServiceTests
{
    private readonly TicketChartService _sut = new();

    private static IEnumerable<Ticket> MakeTickets(params (Status status, TicketPriority priority)[] specs)
        => specs.Select(s => new Ticket { Status = s.status, Priority = s.priority });

    // ── GetStatusChartData ─────────────────────────────────────────────────────

    [Fact]
    public void GetStatusChartData_ReturnsLabelForEveryStatus()
    {
        var result = _sut.GetStatusChartData(MakeTickets());

        result.Labels.Should().BeEquivalentTo(Enum.GetNames(typeof(Status)));
    }

    [Fact]
    public void GetStatusChartData_CountsStatusesCorrectly()
    {
        var tickets = MakeTickets(
            (Status.New, TicketPriority.Low),
            (Status.New, TicketPriority.High),
            (Status.Resolved, TicketPriority.Medium));

        var result = _sut.GetStatusChartData(tickets);

        var newIdx = Array.IndexOf(result.Labels, nameof(Status.New));
        var resolvedIdx = Array.IndexOf(result.Labels, nameof(Status.Resolved));
        result.Data[newIdx].Should().Be(2);
        result.Data[resolvedIdx].Should().Be(1);
    }

    [Fact]
    public void GetStatusChartData_ReturnsZero_ForStatusWithNoTickets()
    {
        var tickets = MakeTickets((Status.New, TicketPriority.Low));

        var result = _sut.GetStatusChartData(tickets);

        var canceledIdx = Array.IndexOf(result.Labels, nameof(Status.Canceled));
        result.Data[canceledIdx].Should().Be(0);
    }

    // ── GetPriorityChartData ────────────────────────────────────────────────────

    [Fact]
    public void GetPriorityChartData_ReturnsLabelForEveryPriority()
    {
        var result = _sut.GetPriorityChartData(MakeTickets());

        result.Labels.Should().BeEquivalentTo(Enum.GetNames(typeof(TicketPriority)));
    }

    [Fact]
    public void GetPriorityChartData_CountsPrioritiesCorrectly()
    {
        var tickets = MakeTickets(
            (Status.New, TicketPriority.High),
            (Status.New, TicketPriority.High),
            (Status.New, TicketPriority.Low));

        var result = _sut.GetPriorityChartData(tickets);

        var highIdx = Array.IndexOf(result.Labels, nameof(TicketPriority.High));
        var lowIdx = Array.IndexOf(result.Labels, nameof(TicketPriority.Low));
        result.Data[highIdx].Should().Be(2);
        result.Data[lowIdx].Should().Be(1);
    }

    [Fact]
    public void GetPriorityChartData_ReturnsZero_ForPriorityWithNoTickets()
    {
        var tickets = MakeTickets((Status.New, TicketPriority.Critical));

        var result = _sut.GetPriorityChartData(tickets);

        var noneIdx = Array.IndexOf(result.Labels, nameof(TicketPriority.None));
        result.Data[noneIdx].Should().Be(0);
    }
}
