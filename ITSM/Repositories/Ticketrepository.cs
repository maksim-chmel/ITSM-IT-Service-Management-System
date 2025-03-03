using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class Ticketrepository(DBaseContext dBaseContext) : ITicketRepository
{
    public async Task CreateNewTicket(Ticket ticket, User user)
    {
        ticket.ContactNumber = user.PhoneNumber;
        ticket.CreatedBy = user.UserName;
        await dBaseContext.Tickets.AddAsync(ticket);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task DeleteTicket(Ticket ticket)
    {
        var closedTickets = await dBaseContext.Tickets
            .Where(a => a.Status == TicketStatus.Closed)
            .ToListAsync();

        dBaseContext.Tickets.RemoveRange(closedTickets);
        await dBaseContext.SaveChangesAsync();
    }

} 