using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Repositories.TicketCategory;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.Ticket;

public class TicketRepository(DBaseContext dBaseContext, ITicketCategoryRepository category)
    : ITicketRepository
{
    public async Task<bool> CreateNewTicket(TicketCreateViewModel model, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(model.Title) && string.IsNullOrWhiteSpace(model.Description)) return false;
        var ticket = new Models.Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            Status = Status.New,
            CategoryId = model.CategoryId,
            TicketSubCategoryId = model.SubCategoryId,
            AuthorId = currentUserId,
            IsDeleted = false
        };
        dBaseContext.Tickets.Add(ticket);
        await dBaseContext.SaveChangesAsync();
        return true;
    }


    public async Task<bool> ResolveTicket(int id, string solution)
    {
        var ticket = await GetTicketById(id);
        if (ticket == null || ticket.Status != Status.Progress) return false;
        ticket.Status = Status.Resolved;
        ticket.ClosedAt = DateTime.Now;
        ticket.FixDescription = solution;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCancelReason(int id, string reason)
    {
        var ticket = await GetTicketById(id);

        if (ticket == null) return false;
        ticket.Status = Status.Canceled;
        ticket.Priority = TicketPriority.None;
        ticket.ClosedAt = DateTime.Now;
        ticket.CancelReason = reason;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Models.Ticket>> GetAllTickets()
    {
        return await dBaseContext.Tickets
            .Where(c => !c.IsDeleted)
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .Include(t => t.AssignedUser)
            .ToListAsync();
    }


    public async Task<IEnumerable<Models.Ticket>> GetTicketsAssignedToAdminAsync(string adminId)
    {
        return await dBaseContext.Tickets
            .Where(c => !c.IsDeleted)
            .Where(t => t.AssignedUserId == adminId)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .Include(t => t.Author)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetUserTickets(string userId)
    {
        return await dBaseContext.Tickets
            .Where(c => !c.IsDeleted)
            .Where(t => t.AuthorId == userId)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .ToListAsync();
    }


    public async Task<Models.Ticket?> GetTicketById(int id)
    {
        return await dBaseContext.Tickets.FindAsync(id);
    }

    public async Task<bool> ChangeTicketStatus(int id, Status status)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(id);
        if (ticket == null) return false;
        ticket.Status = status;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();
        return true;
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

    public async Task<TicketCreateViewModel> BuildCreateTicketViewModel(int selectedCategoryId)
    {
        var categories = await category.GetAllCategoriesToList();
        var subCategories = await category.GetSubCategoriesForCategory(selectedCategoryId);

        var categorySelectList = await category.GetCategorySelectListAsync(categories);

        return new TicketCreateViewModel
        {
            CategoryId = selectedCategoryId,
            Categories = categorySelectList,
            SubCategories = category.MapSubCategoriesToSelectList(subCategories)
        };
    }


    
}