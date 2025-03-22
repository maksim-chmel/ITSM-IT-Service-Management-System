using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class TicketRepository(DBaseContext dBaseContext) : ITicketRepository
{
    public async Task CreateNewTicket(TicketCreateViewModel model, string currentUserId)
    {
        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            Status = model.Status,
            CategoryId = model.CategoryId,
            AuthorId = currentUserId
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

    public async Task CloseTicket(int id)
    {
        var ticket = await GetTicketById(id);
        if (ticket.Status == TicketStatus.New)
        {
            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAt = DateTime.Now;

            dBaseContext.Tickets.Update(ticket);
            await dBaseContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Ticket>> GetAllTickets()
    {
        return await dBaseContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetUserTickets(string userId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AuthorId == userId)
            .Include(t => t.Category)
            .ToListAsync();
    }


    private async Task<Ticket?> GetTicketById(int id)
    {
        return await dBaseContext.Tickets.FindAsync(id);
    }


    private async Task<List<TicketHistory>> GetTicketHistoryByTicketId(int ticketId)
    {
        return await dBaseContext.TicketHistory
            .Where(th => th.TicketId == ticketId)
            .OrderByDescending(th => th.CreatedAt)
            .ToListAsync();
    }


    public async Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId)
    {
        var ticket = await dBaseContext.Tickets
            .Include(t => t.Author)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        var ticketHistory = await GetTicketHistoryByTicketId(ticketId);

        return new TicketDetailsViewModel
        {
            Ticket = ticket,
            TicketHistory = ticketHistory,
            AuthorName = ticket?.Author?.UserName
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

    public async Task<TicketCreateViewModel> AddCategoriesToViewModel()
    {
        var categories = await GetCategorySelectList();
        return new TicketCreateViewModel
        {
            Categories = categories
        };
    }

    private async Task<List<SelectListItem>> GetCategorySelectList()
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