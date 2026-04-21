using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;

namespace ITSM.Services.Ticket;

public interface ITicketService
{
    Task<Models.Ticket?> GetTicketById(int id);
    Task<OperationResult> CreateNewTicket(TicketCreateViewModel model, string currentUserId);
    Task<OperationResult> ResolveTicket(int id, string solution, int? knowledgeBaseArticleId);
    Task<IEnumerable<Models.Ticket>> GetAllTickets();
    IQueryable<Models.Ticket> GetAllTicketsQuery();
    Task<IEnumerable<Models.Ticket>> GetUserTickets(string userId);
    IQueryable<Models.Ticket> GetUserTicketsQuery(string userId);

    Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId);
    Task AddTicketStepAsync(int ticketId, string adminComment);
    Task<IEnumerable<Models.Ticket>> GetTicketsAssignedToAdminAsync(string adminId);
    Task<OperationResult> ChangeTicketStatus(int id, Status status);
    Task<OperationResult> CancelTicketAsync(int id, string reason, string actorUserId);
    Task<TicketCreateViewModel> BuildCreateTicketViewModel(int selectedCategoryId);
    Task<OperationResult> PlaceOnHoldAsync(int ticketId, string reason);
    Task<OperationResult> ResumeProgressAsync(int ticketId);
    Task<OperationResult> ReopenTicketAsync(int ticketId, string reason);
    Task<OperationResult> AcceptTicketProcessingAsync(int ticketId, string technicianUserId);
    Task<OperationResult> ArchiveTicketAsync(int ticketId, string actorUserId, string? note = null);
    Task<OperationResult> RestoreTicketAsync(int ticketId, string actorUserId, string? note = null);
    Task<OperationResult> UnassignTicketAsync(int ticketId);
}
