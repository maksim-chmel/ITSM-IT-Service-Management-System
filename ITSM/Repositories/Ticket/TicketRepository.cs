using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Repositories.TicketCategory;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.Ticket;

public class TicketRepository(DBaseContext dBaseContext, ITicketCategoryRepository ticketCategoryRepository)
    : ITicketRepository
{
    public async Task CreateNewTicket(TicketCreateViewModel model, string currentUserId)
    {
        var ticket = new Models.Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            Status = Status.New,
            CategoryId = model.CategoryId,
            TicketSubCategoryId = model.SubCategoryId,
            AuthorId = currentUserId
        };

        dBaseContext.Tickets.Add(ticket);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task MassDeleteTickets()
    {
        var closedTickets = await dBaseContext.Tickets
            .Where(a => a.Status == Status.Done)
            .ToListAsync();

        dBaseContext.Tickets.RemoveRange(closedTickets);
        await dBaseContext.SaveChangesAsync();
    }

    public async Task ResolveTicket(int id, string solution)
    {
        var ticket = await GetTicketById(id);
        if (ticket.Status == Status.Progress)
        {
            ticket.Status = Status.Resolved;
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
            ticket.Status = Status.Canceled;
            ticket.Priority = TicketPriority.None;
            ticket.ClosedAt = DateTime.Now;
            ticket.CancelReason = reason;
            dBaseContext.Tickets.Update(ticket);
            await dBaseContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Models.Ticket>> GetAllTickets()
    {
        return await dBaseContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .Include(t => t.AssignedUser)
            .ToListAsync();
    }


    public async Task<IEnumerable<Models.Ticket>> GetTicketsAssignedToAdminAsync(string adminId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AssignedUserId == adminId)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .Include(t => t.Author)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetUserTickets(string userId)
    {
        return await dBaseContext.Tickets
            .Where(t => t.AuthorId == userId)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .ToListAsync();
    }


    public async Task<Models.Ticket?> GetTicketById(int id)
    {
        return await dBaseContext.Tickets.FindAsync(id);
    }

    public async Task ChangeTicketStatus(int id, Status status)
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
            .Include(t => t.AssignedUser)
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

    private async Task<List<Models.TicketCategory>> GetCategories()
    {
        return await dBaseContext.TicketCategories.ToListAsync();
    }


    private async Task<List<TicketSubCategory>> GetSubCategories(int categoryId)
    {
        return await dBaseContext.TicketSubCategories
            .Where(sc => sc.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<TicketCreateViewModel> BuildCreateTicketViewModel(int? selectedCategoryId = null)
    {
        var categories = await GetCategories();
        var subCategories = await GetSubCategoriesForCategory(selectedCategoryId);

        return new TicketCreateViewModel
        {
            CategoryId = selectedCategoryId,
            Categories = MapCategoriesToSelectList(categories),
            SubCategories = MapSubCategoriesToSelectList(subCategories)
        };
    }

    private async Task<List<TicketSubCategory>> GetSubCategoriesForCategory(int? selectedCategoryId)
    {
        if (selectedCategoryId.HasValue)
        {
            return await GetSubCategories(selectedCategoryId.Value);
        }

        return new List<TicketSubCategory>();
    }

    private List<SelectListItem> MapCategoriesToSelectList(List<Models.TicketCategory> categories)
    {
        return categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();
    }

    private List<SelectListItem> MapSubCategoriesToSelectList(List<TicketSubCategory> subCategories)
    {
        return subCategories.Select(sc => new SelectListItem
        {
            Value = sc.Id.ToString(),
            Text = sc.Name
        }).ToList();
    }


}