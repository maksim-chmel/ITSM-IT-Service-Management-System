using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.TicketSort;
using ITSM.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Technician))]
public class TechnicianTicketController(
    ITicketService ticketService,
    IUserManagementService userService,
    ITicketSortService ticketSortService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> ToDoTicketsList(int? categoryId, TicketPriority? priority, Status? status)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);
        var list = await ticketService.GetTicketsAssignedToAdminAsync(currentUser.Id);
        list = ticketSortService.GetFilteredTickets(list, categoryId, priority, status);
        ViewBag.Categories = ticketSortService.GetCategorySelectList();

        return View(list);
    }


    [HttpPost]
    public async Task<IActionResult> ResolveTicket(int id, string solution)
    {
      var result = await ticketService.ResolveTicket(id, solution);
      SetTempDataMessage(result, "Ticket resolved successfully.", "Error while resolving the ticket.");

        
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AcceptTicketProcessing(int id)
    {
       var result = await ticketService.ChangeTicketStatus(id, Status.Progress);
       SetTempDataMessage(result, "Action was successful.", "Something went wrong. Please try again.");
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> CancelTicketProcessing(int id)
    {
       var result = await ticketService.ChangeTicketStatus(id, Status.Canceled);
       SetTempDataMessage(result, "Action was successful.", "Something went wrong. Please try again.");
        return RedirectToAction("ToDoTicketsList");
    }

    [HttpGet]
    public async Task<IActionResult> ShowDetailsAboutTicket(int id)
    {
        var viewModel = await ticketService.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddNewCommentToTicket(int ticketId, string adminComment)
    {
        if (string.IsNullOrWhiteSpace(adminComment))
        {
            ModelState.AddModelError("adminComment", "Note is required.");
        }

        await ticketService.AddTicketStepAsync(ticketId, adminComment);
        return RedirectToAction("ShowDetailsAboutTicket", new { id = ticketId });
    }
}