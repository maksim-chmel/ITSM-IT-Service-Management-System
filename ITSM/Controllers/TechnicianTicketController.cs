using ITSM.Enums;
using ITSM.Services.Ticket;
using ITSM.Services.TicketSort;
using ITSM.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSM.Models;
using Microsoft.AspNetCore.Identity;
using ITSM.Authorization;

namespace ITSM.Controllers;

[Authorize]
public class TechnicianTicketController(
    ITicketService ticketService,
    IUserManagementService userService,
    ITicketSortService ticketSortService,
    UserManager<User> userManager,
    IAuthorizationService authorizationService) : BaseController
{
    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Technician)},{nameof(UserRoles.Coordinator)}")]
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
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.Resolve));
        if (!auth.Succeeded)
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
        if (ticket == null) return NotFound();

        // If assigned to someone else, only Admin/Coordinator should be able to proceed.
        if (ticket.AssignedUserId != null)
        {
            var caller = await userManager.GetUserAsync(User);
            if (caller != null && ticket.AssignedUserId != caller.Id)
            {
                var allowOverride =
                    User.IsInRole(nameof(UserRoles.Admin)) ||
                    User.IsInRole(nameof(UserRoles.Coordinator));
                if (!allowOverride)
                {
                    NotifyError("This ticket is already assigned to another technician.");
                    return RedirectToAction("ShowDetailsAboutTicket", new { id });
                }
            }
        }

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.Accept));
        if (!auth.Succeeded)
        {
            NotifyError("You are not authorized to start processing this ticket.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }
        
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Auth");

        var result = await ticketService.AcceptTicketProcessingAsync(id, currentUser.Id);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelTicketProcessing(int id)
    {
        var ticket = await ticketService.GetTicketById(id);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.ChangeStatus));
        if (!auth.Succeeded)
        {
            NotifyError("You are not authorized to cancel processing for this ticket.");
            return RedirectToAction("ShowDetailsAboutTicket", new { id });
        }

        var actor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await ticketService.CancelTicketAsync(id, "Canceled by technician.", actor);
        SetNotification(result);
        return RedirectToAction("ToDoTicketsList");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveTicket(int id, string? note)
    {
        var ticket = await ticketService.GetTicketById(id);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.Archive));
        if (!auth.Succeeded) return Forbid();

        var actor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await ticketService.ArchiveTicketAsync(id, actor, note);
        SetNotification(result);
        return RedirectToAction("ToDoTicketsList");
    }

    [HttpGet]
    public async Task<IActionResult> ShowDetailsAboutTicket(int id)
    {
        var ticket = await ticketService.GetTicketById(id);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.View));
        if (!auth.Succeeded) return Forbid();

        var viewModel = await ticketService.CreateTicketDetailsViewModel(id);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewCommentToTicket(int ticketId, string adminComment)
    {
        var ticket = await ticketService.GetTicketById(ticketId);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.AddComment));
        if (!auth.Succeeded)
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
        var ticket = await ticketService.GetTicketById(id);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.ChangeStatus));
        if (!auth.Succeeded) return Forbid();

        var result = await ticketService.PlaceOnHoldAsync(id, reason);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Coordinator)},{nameof(UserRoles.Technician)}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResumeProgress(int id)
    {
        var ticket = await ticketService.GetTicketById(id);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.ChangeStatus));
        if (!auth.Succeeded) return Forbid();

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
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.Reopen));
        if (!auth.Succeeded)
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
        var ticket = await ticketService.GetTicketById(id);
        if (ticket == null) return NotFound();

        var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.Unassign));
        if (!auth.Succeeded) return Forbid();

        var result = await ticketService.UnassignTicketAsync(id);
        SetNotification(result);
        return RedirectToAction("ShowDetailsAboutTicket", new { id });
    }
}
