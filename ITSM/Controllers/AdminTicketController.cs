using ITSM.Enums;
using ITSM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class AdminTicketController(ITicketRepository ticketRepository,IUserManagementRepository userRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> AllTicketsList()
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        var list = await ticketRepository.GetTicketsAssignedToAdminAsync(currentUser.Id);
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> ResolveTicket(int id, string solution)
    {
        await ticketRepository.ResolveTicket(id, solution);

        TempData["TicketClosed"] = "Заявка успешно решена.";

        return RedirectToAction("Details", new { id });
    }
    
    [HttpPost]
    public async Task<IActionResult> AcceptTicketProcessing (int id)
    {
        await ticketRepository.ChangeTicketStatus(id, TicketStatus.Progress);
        return RedirectToAction("Details", new { id });
    }
    [HttpPost]
    public async Task<IActionResult> CancelTicketProcessing (int id)
    {
        await ticketRepository.ChangeTicketStatus(id, TicketStatus.Canceled);
        return RedirectToAction("AllTicketsList");
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var viewModel = await ticketRepository.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddTicketComment(int ticketId, string adminComment)
    {
        await ticketRepository.AddTicketStepAsync(ticketId, adminComment);
        return RedirectToAction("Details", new { id = ticketId });
    }
}