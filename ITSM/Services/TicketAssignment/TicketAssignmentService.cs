using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Ticket;
using ITSM.Services.UserManagement;
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

    
    public async Task<OperationResult> AssignTicketToTechnician(int ticketId, string userId)
    {
        var ticket = await ticketService.GetTicketById(ticketId);

        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        ticket.Status = Status.Open;
        ticket.AssignedUserId = userId;
        await context.SaveChangesAsync();
        return OperationResult.Success("Ticket assigned to technician.");
    }

   
    public async Task<OperationResult> UpdateTicketPriority(int ticketId, TicketPriority priority)
    {
        var ticket = await ticketService.GetTicketById(ticketId);

        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        ticket.Priority = priority;
        await context.SaveChangesAsync();
        return OperationResult.Success("Ticket priority updated.");
    }
}
