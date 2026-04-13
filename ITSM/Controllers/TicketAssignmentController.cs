using ITSM.Enums;
using ITSM.Services.Automation;
using ITSM.Services.Charts;
using ITSM.Services.Ticket;
using ITSM.Services.TicketAssignment;
using ITSM.Services.TicketSort;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Coordinator))]
public class TicketAssignmentController(
    ITicketService ticketService,
    ITicketAssignmentService ticketAssignmentService,
    ITicketSortService ticketSortService,
    IAutoServiceService serviceService,
    ITicketChartService chart)
    : BaseController
{
    [HttpGet]
    public async Task<IActionResult> ReviewAllTickets(int? categoryId, TicketPriority? priority, Status? status)
    {
        var tickets = await ticketService.GetAllTickets();
        tickets = ticketSortService.GetFilteredTickets(tickets, categoryId, priority, status);
        ViewBag.StatusChartData = chart.GetStatusChartData(tickets);
        ViewBag.PriorityChartData = chart.GetPriorityChartData(tickets);
        ViewBag.Categories = ticketSortService.GetCategorySelectList();
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SelectedPriority = priority;
        ViewBag.SelectedStatus = status;

        return View(tickets);
    }

    [HttpGet]
    public async Task<IActionResult> AssignTechnician(int id)
    {
        var model = await ticketAssignmentService.CreateAssignTechnicianViewModel(id);
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> AssignTechnician(int ticketId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("userId", "Please select a technician.");
            var model = await ticketAssignmentService.CreateAssignTechnicianViewModel(ticketId);
            return View(model);
        }

        var result = await ticketAssignmentService.AssignTicketToTechnician(ticketId, userId);
        SetTempDataMessage(result, "Assigned successfully.", "Error during assignment.");

        return RedirectToAction("ReviewAllTickets");
    }


    [HttpGet]
    public async Task<IActionResult> AssignPriority(int id)
    {
        var model = await ticketAssignmentService.CreateAssignPriorityViewModel(id);
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> AssignPriority(int ticketId, TicketPriority priority)
    {
        if (!Enum.IsDefined(typeof(TicketPriority), priority))
        {
            ModelState.AddModelError("priority", "Invalid priority selected.");
            var model = await ticketAssignmentService.CreateAssignPriorityViewModel(ticketId);
            return View(model);
        }

        var result = await ticketAssignmentService.UpdateTicketPriority(ticketId, priority);
        SetTempDataMessage(result, "Assigned successfully.", "Error during assignment.");

        return RedirectToAction("ReviewAllTickets");
    }

    [HttpGet]
    public async Task<IActionResult> InfoAboutTicket(int id)
    {
        var viewModel = await ticketService.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CloseTicket(int id)
    {
        await ticketService.ChangeTicketStatus(id, Status.Done);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ReOpenTicket(int id)
    {
        await ticketService.ChangeTicketStatus(id, Status.Reopened);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> CancelTicket(int id, string reason)
    {
        await ticketService.AddCancelReason(id, reason);
        return RedirectToAction("ReviewAllTickets");
    }

    [HttpPost]
    public async Task<IActionResult> AssignTickets()
    {
        try
        {
            await serviceService.AssignTicketsByCategoryAndLoadAsync();
            SetTempDataMessage(true, "Tickets were successfully assigned!", "");

            return RedirectToAction("ReviewAllTickets");
        }
        catch (Exception ex)
        {
            SetTempDataMessage(false, "", "Error while assigning tickets: " + ex.Message);

            return RedirectToAction("Error", "Home");
        }
    }


    [HttpPost]
    public async Task<IActionResult> ResetTickets()
    {
        try
        {
            await serviceService.ResetTicketsAsync();
            SetTempDataMessage(true, "Tickets successfully reset!", "");

            return RedirectToAction("ReviewAllTickets");
        }
        catch (Exception ex)
        {
            SetTempDataMessage(false, "", "Error resetting tickets: " + ex.Message);
            return RedirectToAction("Error", "Home");
        }
    }
}