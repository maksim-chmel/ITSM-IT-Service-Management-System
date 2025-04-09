using ITSM.Enums;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.TicketSort;
using ITSM.Repositories.UserManagment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Technician))]
public class TechnicianTicketController(ITicketRepository ticketRepository,IUserManagementRepository userRepository,ITicketSortRepository ticketSortRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ToDoTicketsList(int? categoryId, TicketPriority? priority, Status? status)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        var list = await ticketRepository.GetTicketsAssignedToAdminAsync(currentUser.Id);
             list = ticketSortRepository.GetFilteredTickets(list,categoryId, priority, status);
            ViewBag.Categories = ticketSortRepository.GetCategorySelectList();

        return View(list);
    }
    

    [HttpPost]
    public async Task<IActionResult> ResolveTicket(int id, string solution)
    {
        await ticketRepository.ResolveTicket(id, solution);

        TempData["TicketClosed"] = "Заявка успешно решена.";

        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }
    
    [HttpPost]
    public async Task<IActionResult> AcceptTicketProcessing (int id)
    {
        await ticketRepository.ChangeTicketStatus(id, Status.Progress);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }
    [HttpPost]
    public async Task<IActionResult> CancelTicketProcessing (int id)
    {
        await ticketRepository.ChangeTicketStatus(id, Status.Canceled);
        return RedirectToAction("ToDoTicketsList");
    }
    [HttpGet]
    public async Task<IActionResult> ShowDetailsAboutTicket(int id)
    {
        var viewModel = await ticketRepository.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddNewCommentToTicket(int ticketId, string adminComment)
    {
        await ticketRepository.AddTicketStepAsync(ticketId, adminComment);
        return RedirectToAction("ShowDetailsAboutTicket", new { id = ticketId });
    }
}