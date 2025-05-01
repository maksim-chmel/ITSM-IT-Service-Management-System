using ITSM.Enums;
using ITSM.ViewModels.Create;

namespace ITSM.Repositories.Discussion;

public interface IDiscussionRepository
{
    Task<bool> CreateDiscussion(DiscussionCreateViewModel viewModel, string userId);
    Task<bool> ResolveDiscussion(int discussionId, string userId);
    Task<IEnumerable<Models.Discussion>> GetAllDiscussions(Status status);
    Task<Models.Discussion> GetDiscussionByIdWithMessages(int id);
    Task<bool> AddMessage(string userId, int discusId, string messageContent);
    Task<IEnumerable<Models.Discussion>> GetUserDiscussions(string userId);
    Task<IEnumerable<Models.Discussion>> SearchDiscussion(Status status, string search);



}