using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.TicketSort;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTicket(TicketCreateViewModel model)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);

        if (currentUser == null) return RedirectToAction("CreateTicket");

        var result = await ticketService.CreateNewTicket(model, currentUser.Id);

        SetNotification(result);
        return RedirectToAction("CreateTicket");
    }

    [HttpGet]
    public async Task<IActionResult> UserTicketsList(int? categoryId, Status? status)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);

        if (currentUser == null)
        {
            NotifyError("User not found.");
            return RedirectToAction("Error", "Home");
        }

        var query = ticketService.GetUserTicketsQuery(currentUser.Id);
        var filteredQuery = ticketSortService.GetFilteredTickets(query, categoryId, null, status);
        var filteredTickets = await filteredQuery.ToListAsync();
        
        ViewBag.Categories = ticketSortService.GetCategorySelectList();
        return View(filteredTickets);
    }
}