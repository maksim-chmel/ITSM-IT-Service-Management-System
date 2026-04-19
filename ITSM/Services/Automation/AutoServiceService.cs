using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Automation;

public class AutoServiceService(
    DBaseContext dBaseContext,
    UserManager<User> userManager,
    ILogger<AutoServiceService> logger) : IAutoServiceService
{
    public async Task AssignTicketsByCategoryAndLoadAsync()
    {
        var ticketsToAssign = await dBaseContext.Tickets
            .Where(t => (t.Status == Status.New || t.Status == Status.Open))
            .OrderByDescending(t => t.Priority)
            .ToListAsync();

        var technicianIds = (await userManager.GetUsersInRoleAsync(nameof(UserRoles.Technician)))
            .Select(u => u.Id)
            .ToList();

        var users = await dBaseContext.Users
            .Include(u => u.UserCategoryAssignments)
            .Include(u => u.AssignedTickets)
            .Where(u => technicianIds.Contains(u.Id))
            .ToListAsync();

        // Dictionary to track temporary load during this batch run
        var currentLoad = users.ToDictionary(
            u => u.Id, 
            u => u.AssignedTickets.Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
        );

        foreach (var ticket in ticketsToAssign)
        {
            var bestUser = users
                .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
                .Select(u => new
                {
                    User = u,
                    Score = ((int)ticket.Priority * (int)u.SkillLevel) - currentLoad[u.Id]
                })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (bestUser != null)
            {
                AssignTicketToUser(ticket, bestUser.User);
                currentLoad[bestUser.User.Id]++; // Update load for next ticket in loop
            }
        }

        await dBaseContext.SaveChangesAsync();
        logger.LogInformation("Batch ticket assignment completed.");
    }

    public async Task<OperationResult> AssignTicketToAvailableUserAsync(int ticketId)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");

        var technicianIds = (await userManager.GetUsersInRoleAsync(nameof(UserRoles.Technician)))
            .Select(u => u.Id)
            .ToList();

        var potentialUsers = await dBaseContext.Users
            .Include(u => u.UserCategoryAssignments)
            .Include(u => u.AssignedTickets)
            .Where(u => technicianIds.Contains(u.Id))
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
            .ToListAsync();

        var bestUser = potentialUsers
            .Select(u => new
            {
                User = u,
                Score = ((int)ticket.Priority * (int)u.SkillLevel) -
                        u.AssignedTickets.Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestUser != null)
        {
            AssignTicketToUser(ticket, bestUser.User);
            await dBaseContext.SaveChangesAsync();
            return OperationResult.Success($"Ticket auto-assigned to {bestUser.User.UserName}.");
        }

        return OperationResult.Failure("No available technician found for this category.");
    }

    private void AssignTicketToUser(Models.Ticket ticket, User selectedUser)
    {
        ticket.AssignedUserId = selectedUser.Id;
        ticket.Status = Status.Progress;
        logger.LogInformation($"Ticket {ticket.Id} ({ticket.Title}) assigned to {selectedUser.UserName}.");
    }

    public async Task ResetTicketsAsync()
    {
        var ticketsToReset = await dBaseContext.Tickets
            .Where(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
            .ToListAsync();

        foreach (var ticket in ticketsToReset)
        {
            ticket.AssignedUserId = null;
            ticket.Status = Status.New;
        }

        try
        {
            await dBaseContext.SaveChangesAsync();
            logger.LogInformation("Tickets were reset successfully.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Error while resetting tickets: {ex.Message}");
        }
    }
}