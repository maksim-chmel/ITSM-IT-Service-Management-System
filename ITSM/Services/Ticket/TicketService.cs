using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.Services.Automation;
using ITSM.Services.TicketCategory;
using ITSM.Services.Discussion;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Ticket;

public class TicketService(
    DBaseContext dBaseContext, 
    ITicketCategoryService category,
    IAutoServiceService autoService,
    IDiscussionService discussionService)
    : ITicketService
{
    public async Task<OperationResult> CreateNewTicket(TicketCreateViewModel model, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description)) 
            return OperationResult.Failure("Title and Description are required.");

        if (model.SubCategoryId.HasValue)
        {
            var subCategory = await dBaseContext.TicketSubCategories.FindAsync(model.SubCategoryId.Value);
            if (subCategory == null || subCategory.CategoryId != model.CategoryId)
            {
                return OperationResult.Failure("The selected subcategory does not belong to the selected category.");
            }
        }

        var ticket = new Models.Ticket
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = DateTime.UtcNow,
            Status = Status.New,
            CategoryId = model.CategoryId,
            TicketSubCategoryId = model.SubCategoryId,
            AuthorId = currentUserId,
            IsDeleted = false
        };
        dBaseContext.Tickets.Add(ticket);
        await dBaseContext.SaveChangesAsync();
        
        await autoService.AssignTicketToAvailableUserAsync(ticket.Id);

        return OperationResult.Success("Ticket created successfully.");
    }


    public async Task<OperationResult> ResolveTicket(int id, string solution, int? knowledgeBaseArticleId)
    {
        var ticket = await GetTicketById(id);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        if (ticket.Status != Status.Progress) return OperationResult.Failure("Ticket is not in progress.");
        
        ticket.Status = Status.Resolved;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.FixDescription = solution;
        ticket.KnowledgeBaseArticleId = knowledgeBaseArticleId;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();

        await discussionService.AutoResolveDiscussionsByTicketId(id);

        return OperationResult.Success("Ticket resolved successfully.");
    }

    public async Task<OperationResult> CancelTicketAsync(int id, string reason, string actorUserId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult.Failure("Cancel reason is required.");

        var ticket = await dBaseContext.Tickets
            .Include(t => t.TicketHistory)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        
        ticket.Status = Status.Canceled;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.CancelReason = reason;

        dBaseContext.TicketHistory.Add(new TicketHistory
        {
            TicketId = id,
            AdminComment = $"Ticket canceled by {actorUserId}. Reason: {reason}",
            CreatedAt = DateTime.UtcNow
        });
        
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();

        await discussionService.AutoResolveDiscussionsByTicketId(id);

        return OperationResult.Success("Ticket canceled successfully.");
    }

    public async Task<IEnumerable<Models.Ticket>> GetAllTickets()
    {
        return await GetAllTicketsQuery().ToListAsync();
    }

    public IQueryable<Models.Ticket> GetAllTicketsQuery()
    {
        return dBaseContext.Tickets
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory)
            .Include(t => t.AssignedUser);
    }


    public async Task<IEnumerable<Models.Ticket>> GetTicketsAssignedToAdminAsync(string adminId)
    {
        return await GetAllTicketsQuery()
            .Where(t => t.AssignedUserId == adminId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Ticket>> GetUserTickets(string userId)
    {
        return await GetUserTicketsQuery(userId).ToListAsync();
    }

    public IQueryable<Models.Ticket> GetUserTicketsQuery(string userId)
    {
        return dBaseContext.Tickets
            .Where(t => t.AuthorId == userId)
            .Include(t => t.Category)
            .Include(t => t.TicketSubCategory);
    }


    public async Task<Models.Ticket?> GetTicketById(int id)
    {
        return await dBaseContext.Tickets.FindAsync(id);
    }

    public async Task<OperationResult> ChangeTicketStatus(int id, Status status)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(id);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        ticket.Status = status;
        dBaseContext.Tickets.Update(ticket);
        await dBaseContext.SaveChangesAsync();

        if (status == Status.Resolved || status == Status.Canceled)
        {
            await discussionService.AutoResolveDiscussionsByTicketId(id);
        }

        return OperationResult.Success($"Ticket status updated to {status}.");
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
            .Include(t => t.Discussions)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
            var ticketHistory = await GetTicketHistoryByTicketId(ticketId);
            var articles = await dBaseContext.KnowledgeBaseArticles.ToListAsync();

            return new TicketDetailsViewModel
            {
            Ticket = ticket,
            TicketHistory = ticketHistory,
            AuthorName = ticket?.Author?.UserName,
            Discussions = ticket?.Discussions ?? new List<ITSM.Models.Discussion>(),
            AvailableArticles = articles.Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Article
            }).ToList()
            };
    }


    public async Task AddTicketStepAsync(int ticketId, string adminComment)
    {
        var ticketHistory = new TicketHistory
        {
            TicketId = ticketId,
            AdminComment = adminComment,
            CreatedAt = DateTime.UtcNow
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


    
    public async Task<OperationResult> PlaceOnHoldAsync(int ticketId, string reason)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        if (ticket.Status != Status.Progress) return OperationResult.Failure("Only a ticket in progress can be put on hold.");

        ticket.Status = Status.OnHold;
        await AddTicketStepAsync(ticketId, $"Ticket placed on hold. Reason: {reason}");
        await dBaseContext.SaveChangesAsync();
        
        return OperationResult.Success("Ticket has been placed on hold.");
    }

    public async Task<OperationResult> ResumeProgressAsync(int ticketId)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        if (ticket.Status != Status.OnHold) return OperationResult.Failure("Only a ticket on hold can be resumed.");

        ticket.Status = Status.Progress;
        await AddTicketStepAsync(ticketId, "Ticket progress has been resumed.");
        await dBaseContext.SaveChangesAsync();

        return OperationResult.Success("Ticket progress has been resumed.");
    }

    public async Task<OperationResult> AcceptTicketProcessingAsync(int ticketId, string technicianUserId)
    {
        if (string.IsNullOrWhiteSpace(technicianUserId))
            return OperationResult.Failure("Invalid user.");

        var ticket = await dBaseContext.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");

        // If it's already assigned to someone else, do not allow "taking" it.
        if (ticket.AssignedUserId != null && ticket.AssignedUserId != technicianUserId)
            return OperationResult.Failure("Ticket is assigned to another technician.");

        ticket.AssignedUserId = technicianUserId;
        ticket.Status = Status.Progress;

        dBaseContext.TicketHistory.Add(new TicketHistory
        {
            TicketId = ticketId,
            AdminComment = "Ticket accepted for processing.",
            CreatedAt = DateTime.UtcNow
        });

        await dBaseContext.SaveChangesAsync();

        return OperationResult.Success("Ticket accepted for processing.");
    }

    public async Task<OperationResult> ArchiveTicketAsync(int ticketId, string actorUserId, string? note = null)
    {
        var ticket = await dBaseContext.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        if (ticket.IsDeleted) return OperationResult.Failure("Ticket is already archived.");

        ticket.IsDeleted = true;
        dBaseContext.TicketHistory.Add(new TicketHistory
        {
            TicketId = ticketId,
            AdminComment = $"Ticket archived by {actorUserId}." + (string.IsNullOrWhiteSpace(note) ? "" : $" Note: {note}"),
            CreatedAt = DateTime.UtcNow
        });

        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("Ticket archived.");
    }

    public async Task<OperationResult> RestoreTicketAsync(int ticketId, string actorUserId, string? note = null)
    {
        var ticket = await dBaseContext.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        if (!ticket.IsDeleted) return OperationResult.Failure("Ticket is not archived.");

        ticket.IsDeleted = false;
        dBaseContext.TicketHistory.Add(new TicketHistory
        {
            TicketId = ticketId,
            AdminComment = $"Ticket restored by {actorUserId}." + (string.IsNullOrWhiteSpace(note) ? "" : $" Note: {note}"),
            CreatedAt = DateTime.UtcNow
        });

        await dBaseContext.SaveChangesAsync();
        return OperationResult.Success("Ticket restored.");
    }

    public async Task<OperationResult> ReopenTicketAsync(int ticketId, string reason)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");
        if (ticket.Status != Status.Resolved) return OperationResult.Failure("Only a resolved ticket can be reopened.");

        ticket.Status = Status.Reopened;
        ticket.ClosedAt = null;
        await AddTicketStepAsync(ticketId, $"Ticket reopened. Reason: {reason}");
        await dBaseContext.SaveChangesAsync();
        
        // Optionally, re-assign it automatically
        await autoService.AssignTicketToAvailableUserAsync(ticketId);
        
        return OperationResult.Success("Ticket has been reopened.");
    }

    public async Task<OperationResult> UnassignTicketAsync(int ticketId)
    {
        var ticket = await dBaseContext.Tickets.FindAsync(ticketId);
        if (ticket == null) return OperationResult.Failure("Ticket not found.");

        var previousAssignee = ticket.AssignedUserId;
        if (previousAssignee == null) return OperationResult.Failure("Ticket is not assigned to anyone.");
        
        ticket.AssignedUserId = null;
        ticket.Status = Status.Open;
        
        var user = await dBaseContext.Users.FindAsync(previousAssignee);
        await AddTicketStepAsync(ticketId, $"Ticket has been unassigned from {user?.UserName ?? "N/A"}.");
        await dBaseContext.SaveChangesAsync();

        return OperationResult.Success("Ticket has been unassigned and returned to the queue.");
    }
}
