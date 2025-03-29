using ITSM.Enums;
using ITSM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Coordinator))]
public class TicketAssignmentController(
    ITicketRepository ticketRepository,
    ITicketAssignmentRepository ticketAssignmentRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> ReviewTickets()
    {
        var list = await ticketRepository.GetAllTickets();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> AssignTicket(int id)
    {
        var model = await ticketAssignmentRepository.CreateAssignTicketViewModel(id);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> InfoAboutTicket(int id)
    {
        var viewModel = await ticketRepository.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CloseTicket(int id)
    {
        await ticketRepository.ChangeTicketStatus(id, TicketStatus.Closed);
        return RedirectToAction("InfoAboutTicket", new { id });
    }
    
    [HttpPost]
    public async Task<IActionResult> ReOpenTicket(int id)
    {
        await ticketRepository.ChangeTicketStatus(id, TicketStatus.Reopened);
        return RedirectToAction("InfoAboutTicket", new { id });
    }
    [HttpPost]
    public async Task<IActionResult> CancelTicket(int id,string reason)
    {
        await ticketRepository.AddCancelReason(id,reason);
        return RedirectToAction("ReviewTickets");
    }

    [HttpPost]
    public async Task<IActionResult> AssignTicket(int ticketId, string userId, TicketPriority priority)
    {
        await ticketAssignmentRepository.AssignTicketToUser(ticketId, userId, priority);
        return RedirectToAction("ReviewTickets");
    }
}