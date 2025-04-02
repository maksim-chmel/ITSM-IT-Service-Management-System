using ITSM.Enums;
using ITSM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Coordinator))]
public class TicketAssignmentController(
    ITicketRepository ticketRepository,
    ITicketAssignmentRepository ticketAssignmentRepository,ITicketSortRepository ticketSortRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> ReviewAllTickets()
    {
        var list = await ticketRepository.GetAllTickets();
        list = ticketSortRepository.SortTicketsByNew(list);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> AssignTicket(int id)
    {
        var model = await ticketAssignmentRepository.CreateAssignTicketViewModel(id);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AssignTicket(int ticketId, string userId, TicketPriority priority)
    {
        await ticketAssignmentRepository.AssignTicketToUser(ticketId, userId, priority);
        return RedirectToAction("ReviewAllTickets");
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
        await ticketRepository.ChangeTicketStatus(id, TicketStatus.Done);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ReOpenTicket(int id)
    {
        await ticketRepository.ChangeTicketStatus(id, TicketStatus.Reopened);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> CancelTicket(int id, string reason)
    {
        await ticketRepository.AddCancelReason(id, reason);
        return RedirectToAction("ReviewAllTickets");
    }
}