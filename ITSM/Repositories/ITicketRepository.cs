using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace ITSM.Repositories;

public interface ITicketRepository
{
    Task CreateNewTicket(TicketCreateViewModel model, User author);
    Task MassDeleteTickets();
    Task<TicketDetailsViewModel> CloseTicket(int id);
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<TicketCreateViewModel>> GetUserTickets(string userId);
    Task<Ticket> GetTicketById(int id);
    Task<Ticket> GetTicketWithHistoryAsync(int id);

    Task<List<TicketHistory>> GetTicketHistoryByTicketId(int ticketId);
    Task AddTicketHistoryAsync(TicketHistory ticketHistory);
    Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId);
    Task AddTicketStepAsync(int ticketId, string adminComment);
    Task<IEnumerable<SelectListItem>> GetCategoriesAsync();
}