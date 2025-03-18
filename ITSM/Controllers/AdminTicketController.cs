using ITSM.Models;
using ITSM.Repositories;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = "Admin")]
public class AdminTicketController(ITicketRepository ticketRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> AllTicketsList()
    {
        var list = await ticketRepository.GetAllTickets();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> CloseTicket(int id)
    {
        var ticket = await ticketRepository.CloseTicket(id);

        TempData["TicketClosed"] = "Заявка успешно закрыта.";

        return RedirectToAction("Details", new { id = ticket.Id });
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var viewModel = await ticketRepository.CreateTicketDetailsViewModel(id);
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddTicketStep(int ticketId, string adminComment)
    {
        await ticketRepository.AddTicketStepAsync(ticketId, adminComment);
        return RedirectToAction("Details", new { id = ticketId });
    }

}