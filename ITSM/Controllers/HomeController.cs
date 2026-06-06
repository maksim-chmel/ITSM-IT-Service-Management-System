using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class HomeController(ITicketService ticketService, IUserManagementService userService) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            if (User.IsInRole(nameof(UserRoles.User)))
            {
                var tickets = (await ticketService.GetUserTickets(currentUser.Id)).ToList();
                ViewBag.StatA = tickets.Count(t => t.Status is Status.New or Status.Open or Status.Reopened);
                ViewBag.StatALabel = "Open Requests";
                ViewBag.StatB = tickets.Count(t => t.Status == Status.Progress);
                ViewBag.StatBLabel = "In Progress";
                ViewBag.StatC = tickets.Count(t => t.Status == Status.Resolved);
                ViewBag.StatCLabel = "Resolved";
                ViewBag.StatCColor = "text-success";
            }
            else if (User.IsInRole(nameof(UserRoles.Technician)))
            {
                var tasks = (await ticketService.GetTicketsAssignedToAdminAsync(currentUser.Id)).ToList();
                ViewBag.StatA = tasks.Count(t => t.Status is Status.New or Status.Open or Status.Reopened);
                ViewBag.StatALabel = "Waiting";
                ViewBag.StatB = tasks.Count(t => t.Status == Status.Progress);
                ViewBag.StatBLabel = "In Progress";
                ViewBag.StatC = tasks.Count(t => t.Status == Status.OnHold);
                ViewBag.StatCLabel = "On Hold";
                ViewBag.StatCColor = "text-warning";
            }
            else
            {
                var allTickets = (await ticketService.GetAllTickets()).ToList();
                var active = allTickets.Where(t => t.Status != Status.Resolved && t.Status != Status.Canceled).ToList();
                ViewBag.StatA = active.Count;
                ViewBag.StatALabel = "Active Tickets";
                ViewBag.StatB = active.Count(t => t.AssignedUserId == null);
                ViewBag.StatBLabel = "Unassigned";
                ViewBag.StatC = active.Count(t => t.Status == Status.Progress);
                ViewBag.StatCLabel = "In Progress";
                ViewBag.StatCColor = "";
            }
        }

        return View();
    }

    public IActionResult Privacy() => View();

    public IActionResult Error()
    {
        var errorMessage = TempData["ErrorMessage"] as string;
        ViewData["ErrorMessage"] = errorMessage;
        return View();
    }
}
