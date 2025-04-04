using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories;

public class TicketSortRepository(DBaseContext context ):ITicketSortRepository
{
    public IEnumerable<Ticket> GetFilteredTickets(string categoryId, TicketPriority? priority, TicketStatus? status)
    {
        var tickets = context.Tickets.AsQueryable();

        if (!string.IsNullOrEmpty(categoryId))
            tickets = tickets.Where(t => t.CategoryId.ToString() == categoryId);

        if (priority.HasValue)
            tickets = tickets.Where(t => t.Priority == priority);

        if (status.HasValue)
            tickets = tickets.Where(t => t.Status == status);

        return tickets.ToList();
    }

    public IEnumerable<SelectListItem> GetCategorySelectList()
    {
        return context.TicketCategories
            .Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
    }
}



