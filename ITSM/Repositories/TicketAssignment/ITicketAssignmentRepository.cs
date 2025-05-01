using ITSM.Enums;
using ITSM.ViewModels.Manage;

namespace ITSM.Repositories.TicketAssignment;

public interface ITicketAssignmentRepository
{
   
    Task<AssignTicketPriorityViewModel> CreateAssignPriorityViewModel(int ticketId);
    Task<AssignTicketViewModel> CreateAssignTechnicianViewModel(int ticketId);
    Task<bool> AssignTicketToTechnician(int ticketId, string userId);
    Task<bool> UpdateTicketPriority(int ticketId, TicketPriority priority);
}