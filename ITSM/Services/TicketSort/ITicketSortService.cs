using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Services.TicketSort;

public interface ITicketSortService
{
    IEnumerable<Models.Ticket> GetFilteredTickets(IEnumerable<Models.Ticket> tickets, int? categoryId, TicketPriority? priority,
        Status? status);
    IEnumerable<SelectListItem> GetCategorySelectList();
}