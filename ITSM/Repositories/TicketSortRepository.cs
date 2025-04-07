using ITSM.DB;
using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories;

public class TicketSortRepository(DBaseContext context) : ITicketSortRepository
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

    public IEnumerable<Ticket> GetFilteredTickets(IEnumerable<Ticket> tickets, int? categoryId, TicketPriority? priority,
        TicketStatus? status)
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