using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Services.TicketSort;

public interface ITicketSortService
{
    IQueryable<Models.Ticket> GetFilteredTickets(IQueryable<Models.Ticket> tickets, int? categoryId, TicketPriority? priority,
        Status? status);
    IEnumerable<SelectListItem> GetCategorySelectList();
}