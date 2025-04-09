using ITSM.Enums;
using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.TicketAssignment;

public interface ITicketAssignmentRepository
{
    Task<AssignTicketViewModel> CreateAssignTicketViewModel(int id);
    Task AssignTicketToUser(int ticketId, string userId, TicketPriority priority);
}