using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Create;

namespace ITSM.Services.Discussion;

public interface IDiscussionService
{
    Task<OperationResult> CreateDiscussion(DiscussionCreateViewModel viewModel, string userId);
    Task<OperationResult> ResolveDiscussion(int discussionId, string userId);
    Task<IEnumerable<Models.Discussion>> GetAllDiscussions(Status status);
    Task<Models.Discussion> GetDiscussionByIdWithMessages(int id);
    Task<OperationResult> AddMessage(string userId, int discusId, string messageContent);
    Task<IEnumerable<Models.Discussion>> GetUserDiscussions(string userId);
    Task<IEnumerable<Models.Discussion>> SearchDiscussion(Status status, string search);
    Task<OperationResult> AutoResolveDiscussionsByTicketId(int ticketId);
    Task<OperationResult> SoftDeleteDiscussion(int discussionId);
}