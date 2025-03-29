using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels;


namespace ITSM.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetTicketById(int id);
    Task CreateNewTicket(TicketCreateViewModel model, string currentUserId);
    Task<TicketCreateViewModel> AddCategoriesToViewModel();
    Task MassDeleteTickets();
    Task ResolveTicket(int id, string solution);
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<Ticket>> GetUserTickets(string userId);

    Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId);
    Task AddTicketStepAsync(int ticketId, string adminComment);
    Task<IEnumerable<Ticket>> GetTicketsAssignedToAdminAsync(string adminId);
    Task ChangeTicketStatus(int id, TicketStatus status);
    Task AddCancelReason(int id, string reason);
}