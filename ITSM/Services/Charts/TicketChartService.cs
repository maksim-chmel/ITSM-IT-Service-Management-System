using ITSM.Enums;
using ITSM.ViewModels.Manage;

namespace ITSM.Services.Charts;

public class TicketChartService:ITicketChartService
{
    public ChartData GetStatusChartData(IEnumerable<Models.Ticket> tickets)
    {
        var statusGroups = tickets
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var allStatuses = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
        var labels = allStatuses.Select(s => s.ToString()).ToArray();
        var data = allStatuses.Select(s => statusGroups.ContainsKey(s) ? statusGroups[s] : 0).ToArray();

        var backgroundColors = new[]
        {
            "#f39c12", // New
            "#3498db", // Open
            "#2980b9", // Progress
            "#27ae60", // Resolved
            "#2ecc71", // Done
            "#95a5a6", // Canceled
            "#e74c3c"  // Reopened
        };

        return new ChartData
        {
            Labels = labels,
            Data = data,
            BackgroundColors = backgroundColors
        };
    }

    public ChartData GetPriorityChartData(IEnumerable<Models.Ticket> tickets)
    {
        var priorityGroups = tickets
            .GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());

        var allPriorities = Enum.GetValues(typeof(TicketPriority)).Cast<TicketPriority>().ToList();
        var labels = allPriorities.Select(p => p.ToString()).ToArray();
        var data = allPriorities.Select(p => priorityGroups.ContainsKey(p) ? priorityGroups[p] : 0).ToArray();

        var backgroundColors = new[]
        {
            "#95a5a6", // None
            "#2ecc71", // Low
            "#3498db", // Medium
            "#f39c12", // High
            "#e74c3c"  // Critical
        };

        return new ChartData
        {
            Labels = labels,
            Data = data,
            BackgroundColors = backgroundColors
        };
    }
}