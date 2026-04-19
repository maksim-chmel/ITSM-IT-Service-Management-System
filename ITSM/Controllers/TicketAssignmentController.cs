using ITSM.Enums;
using ITSM.Services.Automation;
using ITSM.Services.Charts;
using ITSM.Services.Ticket;
using ITSM.Services.TicketAssignment;
using ITSM.Services.TicketSort;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var query = ticketService.GetAllTicketsQuery();
        var filteredQuery = ticketSortService.GetFilteredTickets(query, categoryId, priority, status);
        var tickets = await filteredQuery.ToListAsync();
        
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTechnician(int ticketId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            NotifyError("Please select a technician.");
            var model = await ticketAssignmentService.CreateAssignTechnicianViewModel(ticketId);
            return View(model);
        }

        var result = await ticketAssignmentService.AssignTicketToTechnician(ticketId, userId);
        SetNotification(result);

        return RedirectToAction("ReviewAllTickets");
    }


    [HttpGet]
    public async Task<IActionResult> AssignPriority(int id)
    {
        var model = await ticketAssignmentService.CreateAssignPriorityViewModel(id);
        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignPriority(int ticketId, TicketPriority priority)
    {
        if (!Enum.IsDefined(typeof(TicketPriority), priority))
        {
            NotifyError("Invalid priority selected.");
            var model = await ticketAssignmentService.CreateAssignPriorityViewModel(ticketId);
            return View(model);
        }

        var result = await ticketAssignmentService.UpdateTicketPriority(ticketId, priority);
        SetNotification(result);

        return RedirectToAction("ReviewAllTickets");
    }

    [HttpGet]
    public async Task<IActionResult> InfoAboutTicket(int id)
    {
        var viewModel = await ticketService.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseTicket(int id)
    {
        var result = await ticketService.ChangeTicketStatus(id, Status.Resolved);
        SetNotification(result);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReOpenTicket(int id)
    {
        var result = await ticketService.ChangeTicketStatus(id, Status.Reopened);
        SetNotification(result);
        return RedirectToAction("InfoAboutTicket", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelTicket(int id, string reason)
    {
        var result = await ticketService.AddCancelReason(id, reason);
        SetNotification(result);
        return RedirectToAction("ReviewAllTickets");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTickets()
    {
        try
        {
            await serviceService.AssignTicketsByCategoryAndLoadAsync();
            NotifySuccess("Tickets were successfully assigned!");

            return RedirectToAction("ReviewAllTickets");
        }
        catch (Exception ex)
        {
            NotifyError("Error while assigning tickets: " + ex.Message);
            return RedirectToAction("Error", "Home");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetTickets()
    {
        try
        {
            await serviceService.ResetTicketsAsync();
            NotifySuccess("Tickets successfully reset!");

            return RedirectToAction("ReviewAllTickets");
        }
        catch (Exception ex)
        {
            NotifyError("Error resetting tickets: " + ex.Message);
            return RedirectToAction("Error", "Home");
        }
    }
}