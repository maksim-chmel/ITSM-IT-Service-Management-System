using ITSM.Enums;
using ITSM.Models;

namespace ITSM.Repositories;

public class TicketSortRepository:ITicketSortRepository
{
    public IEnumerable<Ticket> SortTicketsByPriority(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderByDescending(t => t.Priority) // Высокий приоритет выше
            .ToList();
    }

    public IEnumerable<Ticket> SortTicketsByNew(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderByDescending(t => t.CreatedAt) // Новые заявки сверху
            .ToList();
    }
    public IEnumerable<Ticket> SortTicketsByStatus(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderByDescending(t => t.Status == TicketStatus.Open) // Открытые заявки сверху
            .ToList();
    }
    public IEnumerable<Ticket> SortTicketsByClosedAt(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderBy(t => t.ClosedAt ?? DateTime.MaxValue) // Сначала те, которые не закрыты (если ClosedAt = null)
            .ToList();
    }
    public IEnumerable<Ticket> SortTicketsByTitle(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderBy(t => t.Title) // Сортировка по названию заявки
            .ToList();
    }
    public IEnumerable<Ticket> SortTicketsByPriorityAndStatus(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderByDescending(t => t.Priority) // Высокий приоритет выше
            .ThenByDescending(t => t.Status == TicketStatus.Open) // Открытые заявки выше
            .ToList();
    }
    public IEnumerable<Ticket> SortTicketsByPriorityAndDate(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderByDescending(t => t.Priority) // Высокий приоритет выше
            .ThenByDescending(t => t.CreatedAt) // Сначала новые заявки
            .ToList();
    }
    public IEnumerable<Ticket> SortTicketsByStatusAndDate(IEnumerable<Ticket> tickets)
    {
        return tickets.OrderByDescending(t => t.Status == TicketStatus.Open) // Открытые заявки сверху
            .ThenByDescending(t => t.CreatedAt) // Сначала новые заявки
            .ToList();
    }



}