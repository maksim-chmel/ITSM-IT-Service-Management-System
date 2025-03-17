using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class TicketRepository(DBaseContext dBaseContext) : ITicketRepository
{
    public async Task CreateNewTicket(TicketCreateViewModel model, User author)
    {
        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            Status = model.Status,
            CategoryId = model.CategoryId,
            Author = author,
        };

        dBaseContext.Tickets.Add(ticket);
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

    public async Task<TicketDetailsViewModel> CloseTicket(int id)
    {
        var ticket = await GetTicketById(id);
        if (ticket == null) throw new ArgumentException("Заявка с указанным ID не найдена.");

        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.Now;

        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();

        var ticketHistory = await GetTicketHistoryByTicketId(id);

        return new TicketDetailsViewModel
        {
            Id = id,
            Ticket = ticket,
            TicketHistory = ticketHistory
        };
    }

    public async Task<IEnumerable<Ticket>> GetAllTickets()
    {
        return await dBaseContext.Tickets
            .Include(t => t.Author)
            .ToListAsync();
    }


    public async Task<IEnumerable<TicketCreateViewModel>> GetUserTickets(string userId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AuthorId == userId)
            .Select(t => new TicketCreateViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }


    public async Task<Ticket> GetTicketById(int id)
    {
        return await dBaseContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);
    }


    public async Task<Ticket> GetTicketWithHistoryAsync(int id)
    {
        return await dBaseContext.Tickets
            .Include(t => t.TicketHistory)
            .FirstOrDefaultAsync(t => t.Id == id);
    }


    public async Task<List<TicketHistory>> GetTicketHistoryByTicketId(int ticketId)
    {
        return await dBaseContext.TicketHistory
            .Where(th => th.TicketId == ticketId)
            .OrderByDescending(th => th.CreatedAt)
            .ToListAsync();
    }

    public async Task AddTicketHistoryAsync(TicketHistory ticketHistory)
    {
        dBaseContext.TicketHistory.Add(ticketHistory);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId)
    {
        var ticket = await GetTicketById(ticketId);
        var ticketHistory = await GetTicketHistoryByTicketId(ticketId);

        return new TicketDetailsViewModel
        {
            Ticket = ticket,
            TicketHistory = ticketHistory
        };
    }

    public async Task AddTicketStepAsync(int ticketId, string adminComment)
    {
        var ticketHistory = new TicketHistory
        {
            TicketId = ticketId,
            AdminComment = adminComment,
            CreatedAt = DateTime.Now
        };

        await dBaseContext.TicketHistory.AddAsync(ticketHistory);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<SelectListItem>> GetCategoriesAsync()
    {
        return await dBaseContext.TicketCategories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }
}