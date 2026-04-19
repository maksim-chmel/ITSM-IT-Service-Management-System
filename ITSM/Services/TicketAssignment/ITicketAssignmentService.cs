using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Manage;

namespace ITSM.Services.TicketAssignment;

public interface ITicketAssignmentService
{
   
    Task<AssignTicketPriorityViewModel> CreateAssignPriorityViewModel(int ticketId);
    Task<AssignTicketViewModel> CreateAssignTechnicianViewModel(int ticketId);
    Task<OperationResult> AssignTicketToTechnician(int ticketId, string userId);
    Task<OperationResult> UpdateTicketPriority(int ticketId, TicketPriority priority);
}