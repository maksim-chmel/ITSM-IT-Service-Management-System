using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.TicketSort;
using ITSM.Services.UserManagment;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.User))]
public class UserTicketController(
    IUserManagementService userService,
    ITicketService ticketService,
    ITicketSortService ticketSortService)
    : BaseController
{
    [HttpGet]
    public async Task<IActionResult> CreateTicket(int categoryId)
    {
        var viewModel = await ticketService.BuildCreateTicketViewModel(categoryId);
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(TicketCreateViewModel model)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);

        if (currentUser == null) return RedirectToAction("CreateTicket");

        var result = await ticketService.CreateNewTicket(model, currentUser.Id);

        SetTempDataMessage(result, "Ticket created successfully.", "Error while creating the ticket.");
        return RedirectToAction("CreateTicket");
    }

    [HttpGet]
    public async Task<IActionResult> UserTicketsList(int? categoryId, Status? status)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);

        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Error", "Home");
        }

        var list = await ticketService.GetUserTickets(currentUser.Id);
        var filteredTickets = ticketSortService.GetFilteredTickets(list, categoryId, null, status);
        ViewBag.Categories = ticketSortService.GetCategorySelectList();
        return View(filteredTickets);
    }
}