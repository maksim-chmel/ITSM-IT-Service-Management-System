using ITSM.Models;

namespace ITSM.Repositories;

public interface ITicketSortRepository
{
    IEnumerable<Ticket> SortTicketsByPriority(IEnumerable<Ticket> tickets);
    IEnumerable<Ticket> SortTicketsByNew(IEnumerable<Ticket> tickets);
}