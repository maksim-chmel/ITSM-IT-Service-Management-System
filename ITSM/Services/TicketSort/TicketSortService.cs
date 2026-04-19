using ITSM.Data;
using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Services.TicketSort;

public class 
    TicketSortService(DBaseContext context) : ITicketSortService
{
    public IEnumerable<SelectListItem> GetCategorySelectList()
    {
        return context.TicketCategories
            .Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
    }

    public IQueryable<Models.Ticket> GetFilteredTickets(IQueryable<Models.Ticket> tickets, int? categoryId, TicketPriority? priority,
        Status? status)
    {
        if (categoryId.HasValue)
        {
            tickets = tickets.Where(t => t.CategoryId == categoryId.Value);
        }
        
        if (priority.HasValue)
        {
            tickets = tickets.Where(t => t.Priority == priority);
        }
        
        if (status.HasValue)
        {
            tickets = tickets.Where(t => t.Status == status);
        }

        return tickets;
    }
}