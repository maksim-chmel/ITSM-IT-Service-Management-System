using ITSM.Enums;
using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.TicketAssignment;

public interface ITicketAssignmentRepository
{
   
    Task<AssignTicketPriorityViewModel> CreateAssignPriorityViewModel(int ticketId);
    Task<AssignTicketViewModel> CreateAssignTechnicianViewModel(int ticketId);
    Task AssignTicketToTechnician(int ticketId, string userId);
    Task UpdateTicketPriority(int ticketId, TicketPriority priority);
}