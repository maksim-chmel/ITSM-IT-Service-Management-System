using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories;

public class TicketRepository(DBaseContext dBaseContext, ITicketCategoryRepository ticketCategoryRepository)
    : ITicketRepository
{
    public async Task CreateNewTicket(TicketCreateViewModel model, string currentUserId)
    {
        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            Status = TicketStatus.New,
            CategoryId = model.CategoryId,
            AuthorId = currentUserId
        };

        dBaseContext.Tickets.Add(ticket);
        await dBaseContext.SaveChangesAsync();
    }


    public async Task MassDeleteTickets()
    {
        var closedTickets = await dBaseContext.Tickets
            .Where(a => a.Status == TicketStatus.Done)
            .ToListAsync();

        dBaseContext.Tickets.RemoveRange(closedTickets);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task ResolveTicket(int id, string solution)
    {
        var ticket = await GetTicketById(id);
        if (ticket.Status == TicketStatus.Progress)
        {
            ticket.Status = TicketStatus.Resolved;
            ticket.ClosedAt = DateTime.Now;
            ticket.FixDescription = solution;
            dBaseContext.Tickets.Update(ticket);
            await dBaseContext.SaveChangesAsync();
        }
    }

    public async Task AddCancelReason(int id, string reason)
    {
        var ticket = await GetTicketById(id);

        if (ticket != null)
        {
            ticket.Status = TicketStatus.Canceled;
            ticket.ClosedAt = DateTime.Now;
            ticket.CancelReason = reason;
            dBaseContext.Tickets.Update(ticket);
            await dBaseContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Ticket>> GetAllTickets()
    {
        return await dBaseContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .ToListAsync();
    }


    public async Task<IEnumerable<Ticket>> GetTicketsAssignedToAdminAsync(string adminId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AssignedUserId == adminId)
            .Include(t => t.Category)
            .Include(t =>t.Author)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetUserTickets(string userId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AuthorId == userId)
            .Include(t => t.Category)
            .ToListAsync();
    }


    public async Task<Ticket?> GetTicketById(int id)
    {
        return await dBaseContext.Tickets.FindAsync(id);
    }

    public async Task ChangeTicketStatus(int id, TicketStatus status)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(id);
        ticket.Status = status;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();
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
            .Include(t=>t.AssignedUser)
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
        var categories = await ticketCategoryRepository.GetCategorySelectList();
        return new TicketCreateViewModel
        {
            Categories = categories
        };
    }
}