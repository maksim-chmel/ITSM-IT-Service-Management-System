using ITSM.Models;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace ITSM.Repositories;

public interface ITicketRepository
{
    Task CreateNewTicket(TicketCreateViewModel model, string currentUserId);
    Task<TicketCreateViewModel> PrepareCreateTicketViewModel();
    Task MassDeleteTickets();
    Task CloseTicket(int id);
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<TicketCreateViewModel>> GetUserTickets(string userId);
    
    Task<TicketDetailsViewModel> CreateTicketDetailsViewModel(int ticketId);
    Task AddTicketStepAsync(int ticketId, string adminComment);
}