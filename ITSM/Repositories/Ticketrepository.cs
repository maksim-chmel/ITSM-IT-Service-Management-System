using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class Ticketrepository(DBaseContext dBaseContext) : ITicketRepository
{
    public async Task CreateNewTicket(TicketViewModel newTicket, User user)
    {
        var ticket = new Ticket
        {
            Title = newTicket.Title,
            Description = newTicket.Description,
            CreatedAt = newTicket.CreatedAt,
            Status = newTicket.Status,
            ContactNumber = user.PhoneNumber,
            AuthorId = user.Id,
            AuthorName = user.UserName
        };
        await dBaseContext.Tickets.AddAsync(ticket);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task MassDeleteTickets()
    {
        var closedTickets = await dBaseContext.Tickets
            .Where(a => a.Status == TicketStatus.Closed)
            .ToListAsync();

        dBaseContext.Tickets.RemoveRange(closedTickets);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task CloseTicket(long id, string fixDescription, string assignedTo)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(id);
        if (ticket == null)
        {
            throw new ArgumentException("Заявка с указанным ID не найдена.");
        }

        ticket.Status = TicketStatus.Closed;
        ticket.FixDescription = fixDescription;
        ticket.ClosedAt = DateTime.Now;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Ticket>> GetAllTickets()
    {
        return await dBaseContext.Tickets.ToListAsync();
    }

    public async Task<IEnumerable<TicketViewModel>> GetUserTickets(string userId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AuthorId == userId)
            .Select(t => new TicketViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }
}