using ITSM.Models;
using ITSM.Repositories;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class TicketController(IUserRepository userRepository, ITicketRepository ticketRepository) : Controller
{
    [HttpGet]
    public IActionResult CreateTicket()
    {
        return View(new TicketViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(TicketViewModel newTicket)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);

        await ticketRepository.CreateNewTicket(newTicket, currentUser);

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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllTicketsList()
    {
        var list = await ticketRepository.GetAllTickets();
        return View(list);
    }
}