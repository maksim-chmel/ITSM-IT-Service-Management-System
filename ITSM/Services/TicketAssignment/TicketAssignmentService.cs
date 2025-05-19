using ITSM.DB;
using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.UserManagment;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Services.TicketAssignment;

public class TicketAssignmentService(
    ITicketService ticketService,
    IUserManagementService userManagementService,
    DBaseContext context)
    : ITicketAssignmentService
{
    
    public async Task<AssignTicketViewModel> CreateAssignTechnicianViewModel(int ticketId)
    {
        var ticket = await ticketService.GetTicketById(ticketId);
        var users = await userManagementService.GetAllUsersToList();
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
        var ticket = await ticketService.GetTicketById(ticketId);
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

    
    public async Task<bool> AssignTicketToTechnician(int ticketId, string userId)
    {
        var ticket = await ticketService.GetTicketById(ticketId);

        if (ticket == null) return false;
        ticket.Status = Status.Open;
        ticket.AssignedUserId = userId;
        await context.SaveChangesAsync();
        return true;

    }

   
    public async Task<bool> UpdateTicketPriority(int ticketId, TicketPriority priority)
    {
        var ticket = await ticketService.GetTicketById(ticketId);

        if (ticket == null) return false;
        ticket.Priority = priority;
        await context.SaveChangesAsync();
        return true;

    }
}
