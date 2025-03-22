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
       
        var category = await dBaseContext.TicketCategories
            .Where(c => c.Id == model.CategoryId)
            .FirstOrDefaultAsync();

      
        string categoryName = category?.Name;

        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            Status = model.Status,
            CategoryId = model.CategoryId,
            CategoryName = categoryName,  
            AuthorId = currentUserId,
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
                CreatedAt = t.CreatedAt,
                CategoryName = t.CategoryName
                
            })
            .ToListAsync();
    }


    private async Task<Ticket> GetTicketById(int id)
    {
        return await dBaseContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);
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

    public async Task<TicketCreateViewModel> PrepareCreateTicketViewModel()
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