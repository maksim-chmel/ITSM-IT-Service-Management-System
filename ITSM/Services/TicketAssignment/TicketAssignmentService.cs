using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Ticket;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.TicketAssignment;

public class TicketAssignmentService(
    ITicketService ticketService,
    DBaseContext context)
    : ITicketAssignmentService
{
    
    public async Task<AssignTicketViewModel> CreateAssignTechnicianViewModel(int ticketId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new Exception("Ticket not found");

        var technicians = await context.Users
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.UserName
            })
            .ToListAsync();

        var model = new AssignTicketViewModel
        {
            Ticket = ticket,
            Users = technicians
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
        var ticket = await context.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");

        var user = await context.Users.FindAsync(userId);
        if (user == null) return OperationResult.Failure("Technician not found.");

        ticket.AssignedUserId = userId;
        ticket.Status = Status.Progress;

        var history = new TicketHistory
        {
            TicketId = ticketId,
            AdminComment = $"Ticket assigned to {user.UserName}.",
            CreatedAt = DateTime.UtcNow
        };
        context.TicketHistory.Add(history);
        
        await context.SaveChangesAsync();
        return OperationResult.Success($"Ticket assigned to {user.UserName}.");
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
