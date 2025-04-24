using ITSM.DB;
using ITSM.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITSM;

public class Automatization(DBaseContext dBaseContext)
{
    public async Task AssignTicketToUserAsync(int ticketId)
    {
        var ticket = await dBaseContext.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId);


        if (ticket == null || ticket.AssignedUserId != null || ticket.CategoryId == null)
            return;


        var candidates = dBaseContext.Users
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
            .Include(user => user.AssignedTickets).ToList()
            .Select(u => new
            {
                u.Id,
                ActiveTicketsCount = u.AssignedTickets
                    .Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
            })
            .OrderBy(u => u.ActiveTicketsCount)
            .ToList();


        if (candidates.Count == 0)
            return;
        
        var selectedUserId = candidates.First().Id;
        ticket.AssignedUserId = selectedUserId;
        ticket.Status = Status.Progress;

        await dBaseContext.SaveChangesAsync();
    }
}