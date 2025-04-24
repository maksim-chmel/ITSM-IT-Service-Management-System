using ITSM.Enums;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.TicketAssignment;
using ITSM.Repositories.TicketSort;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Coordinator))]
public class TicketAssignmentController(
    ITicketRepository ticketRepository,
    ITicketAssignmentRepository ticketAssignmentRepository,
    ITicketSortRepository ticketSortRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> ReviewAllTickets(int? categoryId, TicketPriority? priority, Status? status)
    {
        var list = await ticketRepository.GetAllTickets();
        list = ticketSortRepository.GetFilteredTickets(list, categoryId, priority, status);
        ViewBag.Categories = ticketSortRepository.GetCategorySelectList();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> AssignTechnician(int id)
    {
        var model = await ticketAssignmentRepository.CreateAssignTechnicianViewModel(id);
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> AssignTechnician(int ticketId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("userId", "Please select a technician.");
            var model = await ticketAssignmentRepository.CreateAssignTechnicianViewModel(ticketId);
            return View(model);
        }


        await ticketAssignmentRepository.AssignTicketToTechnician(ticketId, userId);
        return RedirectToAction("ReviewAllTickets");
    }


    [HttpGet]
    public async Task<IActionResult> AssignPriority(int id)
    {
        var model = await ticketAssignmentRepository.CreateAssignPriorityViewModel(id);
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> AssignPriority(int ticketId, TicketPriority priority)
    {
        if (!Enum.IsDefined(typeof(TicketPriority), priority))
        {
            ModelState.AddModelError("priority", "Invalid priority selected.");
            var model = await ticketAssignmentRepository.CreateAssignPriorityViewModel(ticketId);
            return View(model);
        }


        await ticketAssignmentRepository.UpdateTicketPriority(ticketId, priority);
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
        await ticketRepository.ChangeTicketStatus(id, Status.Done);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ReOpenTicket(int id)
    {
        await ticketRepository.ChangeTicketStatus(id, Status.Reopened);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> CancelTicket(int id, string reason)
    {
        await ticketRepository.AddCancelReason(id, reason);
        return RedirectToAction("ReviewAllTickets");
    }
}