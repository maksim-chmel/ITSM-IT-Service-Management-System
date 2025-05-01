using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Create;

namespace ITSM.Repositories.Ticket;

public interface ITicketRepository
{
    Task<Models.Ticket?> GetTicketById(int id);
    Task<bool> CreateNewTicket(TicketCreateViewModel model, string currentUserId);
    Task<bool> ResolveTicket(int id, string solution);
    Task<IEnumerable<Models.Ticket>> GetAllTickets();
    Task<IEnumerable<Models.Ticket>> GetUserTickets(string userId);

    Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId);
    Task AddTicketStepAsync(int ticketId, string adminComment);
    Task<IEnumerable<Models.Ticket>> GetTicketsAssignedToAdminAsync(string adminId);
    Task<bool> ChangeTicketStatus(int id, Status status);
    Task<bool> AddCancelReason(int id, string reason);
    Task<TicketCreateViewModel> BuildCreateTicketViewModel(int selectedCategoryId);
    
}