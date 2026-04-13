using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Automation;
public class AutoServiceService(DBaseContext dBaseContext) : IAutoServiceService
{
    public async Task AssignTicketsByCategoryAndLoadAsync()
    {
        var ticketsToAssign = await dBaseContext.Tickets
            .Where(t => (t.Status == Status.New || t.Status == Status.Open) && t.CategoryId != null)
            .OrderByDescending(t => t.Priority)
            .ToListAsync();

        var users = await dBaseContext.Users
            .Include(u => u.UserCategoryAssignments)
            .Include(u => u.AssignedTickets)
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId != null))
            .ToListAsync();

        var tasks = ticketsToAssign.Select(async ticket =>
        {
            Console.WriteLine($"Processing ticket ID: {ticket.Id}, priority: {ticket.Priority}, category: {ticket.CategoryId}");

            var availableUsers = users
                .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
                .Select(u => new
                {
                    User = u,
                    ActiveTicketCount = u.AssignedTickets.Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled),
                    SkillLevel = (int)u.SkillLevel
                })
                .ToList();

            if (availableUsers.Any())
            {
                var selected = availableUsers
                    .OrderByDescending(u => ((int)ticket.Priority * u.SkillLevel) - u.ActiveTicketCount)
                    .First();


                await AssignTicketToUserAsync(ticket, selected.User);
            }
            else
            {
                Console.WriteLine($"No available users for ticket ID: {ticket.Id}, category: {ticket.CategoryId}");
            }
        });

        await Task.WhenAll(tasks);
        await dBaseContext.SaveChangesAsync();
        Console.WriteLine("All tickets were assigned successfully.");
    }

    private async Task AssignTicketToUserAsync(Models.Ticket ticket, User selectedUser)
    {
        ticket.AssignedUserId = selectedUser.Id;
        ticket.Status = Status.Progress;

        Console.WriteLine($"Ticket {ticket.Id} was assigned to user {selectedUser.UserName}.");

        selectedUser.AssignedTickets.Add(ticket);
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
            Console.WriteLine("Tickets were reset successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while resetting tickets: {ex.Message}");
        }
    }
}
