using ITSM.Enums;
using ITSM.ViewModels;

namespace ITSM.Repositories;

public interface ITicketAssignmentRepository
{
    Task<AssignTicketViewModel> CreateAssignTicketViewModel(int id);
    Task AssignTicketToUser(int ticketId, string userId, TicketPriority priority);
}