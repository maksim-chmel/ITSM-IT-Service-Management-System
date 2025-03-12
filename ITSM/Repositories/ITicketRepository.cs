using ITSM.Models;
using ITSM.ViewModels;


namespace ITSM.Repositories;

public interface ITicketRepository
{
    Task CreateNewTicket(TicketViewModel ticket, User user);
    Task MassDeleteTickets();
    Task CloseTicket(long id, string fixDescription, string assignedTo);
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<TicketViewModel>> GetUserTickets(string userId);
}