using ITSM.Enums;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.TicketAssignment;
using ITSM.Repositories.TicketSort;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Coordinator))]
public class TicketAssignmentController(
    ITicketService ticketService,
    ITicketAssignmentService ticketAssignmentService,
    ITicketSortService ticketSortService,
    IAutoServiceService serviceService)
    : Controller
{
    private void SetTempDataMessage(bool isSuccess, string successMessage, string errorMessage)
    {
        if (isSuccess)
        {
            TempData["SuccessMessage"] = successMessage;
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }
    }

    [HttpGet]
    public async Task<IActionResult> ReviewAllTickets(int? categoryId, TicketPriority? priority, Status? status)
    {
        var list = await ticketService.GetAllTickets();
        list = ticketSortService.GetFilteredTickets(list, categoryId, priority, status);
        ViewBag.Categories = ticketSortService.GetCategorySelectList();
        return View(list);
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
        SetTempDataMessage(result, "Назначен успешно.",
            "Ошибка при назначении.");
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
        SetTempDataMessage(result, "Назначен успешно.",
            "Ошибка при назначении.");
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
            TempData["SuccessMessage"] = "Tickets assigned successfully!";
            return RedirectToAction("ReviewAllTickets");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error during ticket assignment: " + ex.Message;
            return RedirectToAction("Error", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ResetTickets()
    {
        try
        {
            await serviceService.ResetTicketsAsync();

            TempData["SuccessMessage"] = "Tickets reset successfully!";
            return RedirectToAction("ReviewAllTickets");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error during ticket reset: " + ex.Message;
            return RedirectToAction("Error", "Home");
        }
    }
}