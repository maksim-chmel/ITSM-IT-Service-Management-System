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
    ITicketSortRepository ticketSortRepository)
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
    public async Task<IActionResult> CreateTicket(int categoryId)
    {
        var viewModel = await ticketRepository.BuildCreateTicketViewModel(categoryId);
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(TicketCreateViewModel model)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        
        if (currentUser == null) return RedirectToAction("CreateTicket");
        
        var result = await ticketRepository.CreateNewTicket(model, currentUser.Id);
        
        SetTempDataMessage(result, "Тикет успешно создан.", "Ошибка при создании тикета.");
        
        return RedirectToAction("CreateTicket");
    }

    [HttpGet]
    public async Task<IActionResult> UserTicketsList(int? categoryId, Status? status)
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
}