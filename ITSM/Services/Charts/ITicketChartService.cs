using ITSM.ViewModels.Manage;

namespace ITSM.Services.Charts;

public interface ITicketChartService
{
    ChartData GetStatusChartData(IEnumerable<Models.Ticket> tickets);
    ChartData GetPriorityChartData(IEnumerable<Models.Ticket> tickets);
}