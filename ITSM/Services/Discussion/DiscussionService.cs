using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.Discussion;

public class DiscussionService(DBaseContext context) : IDiscussionService
{
    public async Task<bool> CreateDiscussion(DiscussionCreateViewModel viewModel, string userId)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Title) && string.IsNullOrWhiteSpace(viewModel.Description))
            return false;

        var newDiscussion = new Models.Discussion
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            CreatedAt = DateTime.Now,
            AuthorId = userId,
            CategoryId = viewModel.CategoryId
        };

        await context.Discussions.AddAsync(newDiscussion);
        await context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> ResolveDiscussion(int discussionId, string userId)
    {
        var discussion = await context.Discussions.FindAsync(discussionId);

        if (discussion == null || discussion.AuthorId != userId) return false;
        discussion.Status = Status.Resolved;
        discussion.ClosedAt = DateTime.Now;
        discussion.IsDeleted = true;
        context.Discussions.Update(discussion);
        await context.SaveChangesAsync();
        return true;

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


    public async Task<bool> AddMessage(string userId, int discusId, string messageContent)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(messageContent)) return false;
        var message = new DiscussionMessage
        {
            AuthorId = userId,
            CreatedAt = DateTime.Now,
            DiscussionId = discusId,
            Content = messageContent
        };

        await context.DiscussionMessages.AddAsync(message);
        await context.SaveChangesAsync();
        return true;
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
}