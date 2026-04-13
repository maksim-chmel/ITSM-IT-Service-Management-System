using ITSM.Data;
using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Services.TicketSort;

public class TicketSortService(DBaseContext context) : ITicketSortService
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

    public IEnumerable<Models.Ticket> GetFilteredTickets(IEnumerable<Models.Ticket> tickets, int? categoryId, TicketPriority? priority,
        Status? status)
    {
        if (!categoryId.HasValue && !priority.HasValue && !status.HasValue)
        {
            return tickets;
        }
        var filteredTickets = tickets.AsQueryable();
        
        if (categoryId.HasValue)
        {
            filteredTickets = filteredTickets.Where(t => t.CategoryId == categoryId.Value);
        }
        
        if (priority.HasValue)
        {
            filteredTickets = filteredTickets.Where(t => t.Priority == priority);
        }
        
        if (status.HasValue)
        {
            filteredTickets = filteredTickets.Where(t => t.Status == status);
        }

        return filteredTickets.ToList();
    }
}