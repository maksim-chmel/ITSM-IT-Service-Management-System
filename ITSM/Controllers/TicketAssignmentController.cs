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


    [HttpPost]
    public async Task<IActionResult> AssignTicket(int ticketId, string userId)
    {
        await ticketAssignmentRepository.AssignTicketToUser(ticketId, userId);
        return RedirectToAction("ReviewTickets");
    }
}