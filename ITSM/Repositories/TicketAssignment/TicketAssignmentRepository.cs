using ITSM.DB;
using ITSM.Enums;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories.TicketAssignment;

public class TicketAssignmentRepository(
    ITicketRepository ticketRepository,
    IUserManagementRepository userManagementRepository,
    DBaseContext context)
    : ITicketAssignmentRepository
{
    
    public async Task<AssignTicketViewModel> CreateAssignTechnicianViewModel(int ticketId)
    {
        var ticket = await ticketRepository.GetTicketById(ticketId);
        var users = await userManagementRepository.GetAllUsersToList();
        var technicians = users
            .Where(u => u.Roles.Contains(nameof(UserRoles.Technician)))
            .ToList();

        var model = new AssignTicketViewModel
        {
            Ticket = ticket,
            Users = technicians.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList()
        };

        return model;
    }
    
    public async Task<AssignTicketPriorityViewModel> CreateAssignPriorityViewModel(int ticketId)
    {
        var ticket = await ticketRepository.GetTicketById(ticketId);
        var priorities = Enum.GetValues(typeof(TicketPriority))
            .Cast<TicketPriority>()
            .Select(p => new SelectListItem
            {
                Value = p.ToString(),
                Text = p.ToString()
            }).ToList();

        var model = new AssignTicketPriorityViewModel
        {
            Ticket = ticket,
            Priorities = priorities
        };

        return model;
    }

    
    public async Task AssignTicketToTechnician(int ticketId, string userId)
    {
        var ticket = await ticketRepository.GetTicketById(ticketId);

        if (ticket != null)
        {
            ticket.Status = Status.Open;
            ticket.AssignedUserId = userId;
        }

        await context.SaveChangesAsync();
    }

   
    public async Task UpdateTicketPriority(int ticketId, TicketPriority priority)
    {
        var ticket = await ticketRepository.GetTicketById(ticketId);

        if (ticket != null)
        {
            ticket.Priority = priority;
        }

        await context.SaveChangesAsync();
    }
}
