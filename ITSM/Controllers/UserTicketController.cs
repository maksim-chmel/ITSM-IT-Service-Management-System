using ITSM.DB;
using ITSM.Enums;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.TicketSort;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.User))]
public class UserTicketController(
    IUserManagementRepository userRepository,
    ITicketRepository ticketRepository,
    ITicketSortRepository ticketSortRepository,
    DBaseContext dBaseContext)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> CreateTicket(int? categoryId)
    {
        var viewModel = await ticketRepository.BuildCreateTicketViewModel(categoryId);
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(TicketCreateViewModel model)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            await ticketRepository.CreateNewTicket(model, currentUser.Id);
            TempData["SuccessMessage"] = "Ticket created.";
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("CreateTicket");
    }

    [HttpGet]
    public async Task<IActionResult> UserTicketsList(int? categoryId, Status? status)
    {
        try
        {
            var currentUser = await userRepository.GetCurrentUserAsync(User);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Error", "Home");
            }

            var list = await ticketRepository.GetUserTickets(currentUser.Id);
            var filteredTickets = ticketSortRepository.GetFilteredTickets(list, categoryId, null, status);
            ViewBag.Categories = ticketSortRepository.GetCategorySelectList();
            return View(filteredTickets);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке тикетов.";
            return RedirectToAction("Error", "Home");
        }
    }
}