using ITSM.DB;
using ITSM.Enums;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.TicketSort;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.User))]
public class UserTicketController(IUserManagementRepository userRepository, ITicketRepository ticketRepository
,ITicketSortRepository ticketSortRepository,DBaseContext dBaseContext)
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
            await AssignTicketToUserAsync(model.Id);
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("CreateTicket");
    }
    [HttpGet]
    public async Task<IActionResult> UserTicketsList(int? categoryId,Status? status)
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
    public async Task AssignTicketToUserAsync(int ticketId)
    {
        var ticket = await dBaseContext.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        // Проверка: заявка не существует, уже назначена или не указана категория
        if (ticket == null || ticket.AssignedUserId != null || ticket.CategoryId == null)
            return;

        // Находим пользователей, у которых есть нужная категория
        var candidates = dBaseContext.Users
            .Where(u => u.UserCategoryAssignments.Any(uca => uca.CategoryId == ticket.CategoryId))
            .Include(user => user.AssignedTickets).ToList()
            .Select(u => new
            {
                u.Id,
                ActiveTicketsCount = u.AssignedTickets
                    .Count(t => t.Status != Status.Resolved && t.Status != Status.Canceled)
            })
            .OrderBy(u => u.ActiveTicketsCount)
            .ToList();

        // Если подходящих нет — выходим
        if (candidates.Count == 0)
            return;

        // Назначаем пользователя с наименьшей загрузкой
        var selectedUserId = candidates.First().Id;
        ticket.AssignedUserId = selectedUserId;
        ticket.Status = Status.Progress;

        await dBaseContext.SaveChangesAsync();
    }

}