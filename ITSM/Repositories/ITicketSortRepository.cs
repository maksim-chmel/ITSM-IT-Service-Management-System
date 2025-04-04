using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.Repositories;

public interface ITicketSortRepository
{
    IEnumerable<Ticket> GetFilteredTickets(string categoryId, TicketPriority? priority, TicketStatus? status);
    
    IEnumerable<SelectListItem> GetCategorySelectList();
}