using ITSM.Data;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Services.Discussion;

public class DiscussionService(DBaseContext context) : IDiscussionService
{
    public async Task<OperationResult> CreateDiscussion(DiscussionCreateViewModel viewModel, string userId)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Title) || string.IsNullOrWhiteSpace(viewModel.Description))
            return OperationResult.Failure("Title and Description are required.");

        var newDiscussion = new Models.Discussion
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            CreatedAt = DateTime.UtcNow,
            AuthorId = userId,
            CategoryId = viewModel.CategoryId,
            TicketId = viewModel.TicketId
        };

        await context.Discussions.AddAsync(newDiscussion);
        await context.SaveChangesAsync();
        return OperationResult.Success("Discussion created successfully.");
    }


    public async Task<OperationResult> ResolveDiscussion(int discussionId, string userId)
    {
        var discussion = await context.Discussions
            .Include(d => d.Messages)
            .FirstOrDefaultAsync(d => d.Id == discussionId);

        if (discussion == null) return OperationResult.Failure("Discussion not found.");
        if (discussion.AuthorId != userId) return OperationResult.Failure("Only the author can resolve this discussion.");
        
        discussion.Status = Status.Resolved;
        discussion.ClosedAt = DateTime.UtcNow;
        discussion.IsDeleted = true;

        context.Discussions.Update(discussion);
        await context.SaveChangesAsync();
        return OperationResult.Success("Discussion has been resolved and moved to the archive.");
    }

    public async Task<IEnumerable<Models.Discussion>> GetAllDiscussions(Status status)
    {
        return await context.Discussions
            .Where(t => t.Status == status)
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .ToListAsync();
    }

    public async Task<Models.Discussion> GetDiscussionByIdWithMessages(int id)
    {
        var discussion = await context.Discussions
            .Include(t => t.Author)
            .Include(t => t.Messages)
            .ThenInclude(m => m.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
        return discussion;
    }


    public async Task<OperationResult> AddMessage(string userId, int discusId, string messageContent)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(messageContent)) 
            return OperationResult.Failure("Message content cannot be empty.");
            
        var message = new DiscussionMessage
        {
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow,
            DiscussionId = discusId,
            Content = messageContent
        };

        await context.DiscussionMessages.AddAsync(message);
        await context.SaveChangesAsync();
        return OperationResult.Success("Message added successfully.");
    }

    public async Task<IEnumerable<Models.Discussion>> GetUserDiscussions(string userId)
    {
        return await context.Discussions
            .Where(t => t.AuthorId == userId)
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Discussion>> SearchDiscussion(Status status, string search)
    {
        search = search.ToLower();
        return await context.Discussions
            .Where(t => !string.IsNullOrEmpty(t.Title) && t.Title.ToLower().Contains(search))
            .Where(t => t.Status == status)
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .ToListAsync();
    }

    public async Task<OperationResult> AutoResolveDiscussionsByTicketId(int ticketId)
    {
        var discussions = await context.Discussions
            .Where(d => d.TicketId == ticketId && d.Status == Status.Open)
            .ToListAsync();

        if (!discussions.Any()) return OperationResult.Success("No open discussions found for this ticket.");

        foreach (var discussion in discussions)
        {
            discussion.Status = Status.Resolved;
            discussion.ClosedAt = DateTime.UtcNow;
        }

        context.Discussions.UpdateRange(discussions);
        await context.SaveChangesAsync();
        return OperationResult.Success($"{discussions.Count} discussions have been automatically resolved.");
    }

    public async Task<OperationResult> SoftDeleteDiscussion(int discussionId)
    {
        var discussion = await context.Discussions
            .Include(d => d.Messages)
            .FirstOrDefaultAsync(d => d.Id == discussionId);

        if (discussion == null) return OperationResult.Failure("Discussion not found.");

        discussion.IsDeleted = true;
        foreach (var message in discussion.Messages)
        {
            message.IsDeleted = true;
        }

        await context.SaveChangesAsync();
        return OperationResult.Success("Discussion and its messages have been archived.");
    }
}
