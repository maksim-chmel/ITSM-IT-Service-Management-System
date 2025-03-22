using ITSM.Repositories;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class UserTicketController(IUserManagementRepository userRepository, ITicketRepository ticketRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> CreateTicket()
    {
        var viewModel = await ticketRepository.AddCategoriesToViewModel();

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(TicketCreateViewModel newTicketCreate)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);

        if (currentUser != null) await ticketRepository.CreateNewTicket(newTicketCreate, currentUser.Id);

        return RedirectToAction("CreateTicket");
    }

    [HttpGet]
    public async Task<IActionResult> UserTicketsList()
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);

        if (currentUser == null) throw new Exception("User not found");

        var list = await ticketRepository.GetUserTickets(currentUser.Id);

        return View(list);
    }
}