using ITSM.Models;


namespace ITSM.Repositories;

public interface ITicketRepository
{
    Task CreateNewTicket(Ticket ticket, User user);
    Task DeleteTicket(Ticket ticket);
}