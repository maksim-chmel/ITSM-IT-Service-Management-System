using ITSM.ViewModels.Create;

namespace ITSM.Repositories.Discussion;

public interface IDiscussionRepository
{
    Task CreateDiscussion(DiscussionCreateViewModel viewModel, string userId);
    Task CloseDiscussion(int discussionId, string userId);
    Task<IEnumerable<Models.Discussion>> GetAllDiscussions();
    Task<Models.Discussion> GetDiscussionByIdWithMessages(int id);
    Task AddMessage(string userId, int discusId, string messageContent);


}