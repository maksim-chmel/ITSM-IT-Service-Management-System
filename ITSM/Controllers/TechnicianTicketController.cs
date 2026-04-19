using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.TicketSort;
using ITSM.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Controllers;

[Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Technician)},{nameof(UserRoles.Coordinator)}")]
public class TechnicianTicketController(
    ITicketService ticketService,
    IUserManagementService userService,
    ITicketSortService ticketSortService,
    UserManager<User> userManager) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> ToDoTicketsList(int? categoryId, TicketPriority? priority, Status? status)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Auth");
        var query = ticketService.GetAllTicketsQuery()
            .Where(t => t.AssignedUserId == currentUser.Id);
            
        var filteredQuery = ticketSortService.GetFilteredTickets(query, categoryId, priority, status);
        var list = await filteredQuery.ToListAsync();
        
        ViewBag.Categories = ticketSortService.GetCategorySelectList();

        return View(list);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveTicket(int id, string solution, int? knowledgeBaseArticleId)
    {
        var ticket = await ticketService.GetTicketById(id);
        var currentUser = await userManager.GetUserAsync(User);
        var userRoles = await userManager.GetRolesAsync(currentUser);

        if (ticket != null && ticket.AssignedUserId != currentUser.Id && !userRoles.Contains(nameof(UserRoles.Admin)) && !userRoles.Contains(nameof(UserRoles.Coordinator)))
        {
            NotifyError("You are not authorized to resolve this ticket.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }
        
        var result = await ticketService.ResolveTicket(id, solution, knowledgeBaseArticleId);
        SetNotification(result);

        
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptTicketProcessing(int id)
    {
        var ticket = await ticketService.GetTicketById(id);
        var currentUser = await userManager.GetUserAsync(User);
        var userRoles = await userManager.GetRolesAsync(currentUser);

        if (ticket != null && ticket.AssignedUserId != null && ticket.AssignedUserId != currentUser.Id && !userRoles.Contains(nameof(UserRoles.Admin)) && !userRoles.Contains(nameof(UserRoles.Coordinator)))
        {
            NotifyError("This ticket is already assigned to another technician.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }
        
       var result = await ticketService.ChangeTicketStatus(id, Status.Progress);
       SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelTicketProcessing(int id)
    {
       var result = await ticketService.ChangeTicketStatus(id, Status.Canceled);
       SetNotification(result);
        return RedirectToAction("ToDoTicketsList");
    }

    [HttpGet]
    public async Task<IActionResult> ShowDetailsAboutTicket(int id)
    {
        var viewModel = await ticketService.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewCommentToTicket(int ticketId, string adminComment)
    {
        var ticket = await ticketService.GetTicketById(ticketId);
        var currentUser = await userManager.GetUserAsync(User);
        var userRoles = await userManager.GetRolesAsync(currentUser);

        if (ticket != null && ticket.AssignedUserId != currentUser.Id && !userRoles.Contains(nameof(UserRoles.Admin)) && !userRoles.Contains(nameof(UserRoles.Coordinator)))
        {
            NotifyError("You are not authorized to add comments to this ticket.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id = ticketId });
        }
        
        if (string.IsNullOrWhiteSpace(adminComment))
        {
            ModelState.AddModelError("adminComment", "Note is required.");
        }

        await ticketService.AddTicketStepAsync(ticketId, adminComment);
        return RedirectToAction("ShowDetailsAboutTicket", new { id = ticketId });
    }
    
    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Coordinator)},{nameof(UserRoles.Technician)}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOnHold(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            NotifyError("A reason is required to place the ticket on hold.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }
        var result = await ticketService.PlaceOnHoldAsync(id, reason);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Coordinator)},{nameof(UserRoles.Technician)}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResumeProgress(int id)
    {
        var result = await ticketService.ResumeProgressAsync(id);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.User)}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReopenTicket(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            NotifyError("A reason is required to reopen the ticket.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }
        
        var ticket = await ticketService.GetTicketById(id);
        var currentUser = await userManager.GetUserAsync(User);
        if (ticket.AuthorId != currentUser.Id && !await userManager.IsInRoleAsync(currentUser, nameof(UserRoles.Admin)))
        {
            NotifyError("You are not authorized to reopen this ticket.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }

        var result = await ticketService.ReopenTicketAsync(id, reason);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Coordinator)}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnassignTicket(int id)
    {
        var result = await ticketService.UnassignTicketAsync(id);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }
}