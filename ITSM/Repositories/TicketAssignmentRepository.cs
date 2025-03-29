using ITSM.DB;
using ITSM.Enums;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories;

public class TicketAssignmentRepository(
    ITicketRepository ticketRepository,
    IUserManagementRepository userManagementRepository,
    DBaseContext context) : ITicketAssignmentRepository
{
    public async Task<AssignTicketViewModel> CreateAssignTicketViewModel(int id)
    {
        var ticket = await ticketRepository.GetTicketById(id);
        var users = await userManagementRepository.GetAllUsersToList();
        var technicians = users
            .Where(u => u.Roles.Contains(nameof(UserRoles.Technician)))
            .ToList();
        var priorities = Enum.GetValues(typeof(TicketPriority))
            .Cast<TicketPriority>()
            .Select(p => new SelectListItem
            {
                Value = p.ToString(),
                Text = p.ToString() 
            }).ToList();

        var model = new AssignTicketViewModel
        {
            Ticket = ticket,
            Users = technicians.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList(),
            Priorities = priorities
            
        };

        return model;
    }
    
    public async Task AssignTicketToUser(int ticketId, string userId,TicketPriority priority)
    {
        var ticket = await ticketRepository.GetTicketById(ticketId);

        if (ticket != null)
        {
            ticket.Status = TicketStatus.Open;
            ticket.AssignedUserId = userId;
            ticket.Priority = priority;
        }

        await context.SaveChangesAsync();
    }
}