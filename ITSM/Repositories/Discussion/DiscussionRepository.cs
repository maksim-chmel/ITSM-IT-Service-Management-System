using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Create;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Repositories.Discussion;

public class DiscussionRepository(DBaseContext context) : IDiscussionRepository
{
    public async Task CreateDiscussion(DiscussionCreateViewModel viewModel, string userId)
    {
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
    }


    public async Task CloseDiscussion(int discussionId, string userId)
    {
        var discussion = await context.Discussions.FindAsync(discussionId);

        if (discussion != null && discussion.AuthorId == userId)
        {
            discussion.Status = Status.Resolved;
            discussion.ClosedAt = DateTime.Now;
            context.Discussions.Update(discussion);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Models.Discussion>> GetAllDiscussions()
    {
        return await context.Discussions
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



    public async Task AddMessage(string userId, int discusId,string messageContent)
    {
        var message = new DiscussionMessage
        {
            AuthorId = userId,
            CreatedAt = DateTime.Now,
            DiscussionId = discusId,
            Content = messageContent
        };

        await context.DiscussionMessages.AddAsync(message);
        await context.SaveChangesAsync();
    }
}