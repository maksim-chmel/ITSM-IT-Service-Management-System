using ITSM.Models;
using ITSM.ViewModels;


namespace ITSM.Repositories;

public interface ITicketRepository
{
    Task CreateNewTicket(TicketCreateViewModel model, string currentUserId);
    Task<TicketCreateViewModel> AddCategoriesToViewModel();
    Task MassDeleteTickets();
    Task CloseTicket(int id);
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<Ticket>> GetUserTickets(string userId);
    
    Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId);
    Task AddTicketStepAsync(int ticketId, string adminComment);
}