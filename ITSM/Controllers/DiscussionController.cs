using ITSM.Data;
using ITSM.Enums;
using ITSM.Services.Discussion;
using ITSM.Services.TicketCategory;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Controllers;

[Authorize(Roles = "Admin,Coordinator,Technician")]
public class DiscussionController(
    ITicketCategoryService categoryService,
    IDiscussionService discussionService,
    IUserManagementService userManagementService,
    DBaseContext dBaseContext,
    ILogger<DiscussionController> logger) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> ListOfDiscussions(string? search)
    {
        var listOfTreads = await discussionService.GetAllDiscussions(Status.Open);
        if (search != null)
        {
            listOfTreads = await discussionService.SearchDiscussion(Status.Open, search);
        }

        return View(listOfTreads);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDiscussion(int? ticketId)
    {
        var categories = await categoryService.GetCategorySelectListAsync();
        var model = new DiscussionCreateViewModel
        {
            Categories = categories,
            TicketId = ticketId
        };

        if (ticketId.HasValue)
        {
            var ticket = await dBaseContext.Tickets.FindAsync(ticketId.Value);
            if (ticket != null)
            {
                model.Title = $"Discussion for Ticket #{ticket.Id}: {ticket.Title}";
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDiscussion(DiscussionCreateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // Log the validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                logger.LogError($"Validation Error: {error.ErrorMessage}");
            }
            
            viewModel.Categories = await categoryService.GetCategorySelectListAsync();
            return View(viewModel);
        }

        if (viewModel.TicketId.HasValue)
        {
            var ticketExists = await dBaseContext.Tickets.AnyAsync(t => t.Id == viewModel.TicketId.Value);
            if (!ticketExists)
            {
                NotifyError("The ticket you are trying to link does not exist.");
                viewModel.Categories = await categoryService.GetCategorySelectListAsync();
                return View(viewModel);
            }
        }

        var user = await userManagementService.GetCurrentUserAsync(User);
        if (user == null)
        {
            NotifyError("User not found.");
            return RedirectToAction("ListOfDiscussions");
        }

        var result = await discussionService.CreateDiscussion(viewModel, user.Id);
        SetNotification(result);

        return RedirectToAction("ListOfDiscussions");
    }




    [HttpGet]
    public async Task<IActionResult> ViewDiscussion(int id)
    {
        var discussionThread = await discussionService.GetDiscussionByIdWithMessages(id);
        return View(discussionThread);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMessage(int id, DiscussionMessageCreateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyError("Please enter a valid message.");
            return RedirectToAction("ViewDiscussion", new { id });
        }

        var user = await userManagementService.GetCurrentUserAsync(User);
        if (user == null)
        {
            NotifyError("User not found.");
            return RedirectToAction("ViewDiscussion", new { id });
        }

        var result = await discussionService.AddMessage(user.Id, id, viewModel.MessageContent);
        SetNotification(result);

        return RedirectToAction("ViewDiscussion", new { id });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveDiscussion(int id)
    {
        var user = await userManagementService.GetCurrentUserAsync(User);
        if (user != null)
        {
            var result = await discussionService.ResolveDiscussion(id, user.Id);
            SetNotification(result);
        }
        else
        {
            NotifyError("User not found.");
        }

        return RedirectToAction("ListOfDiscussions");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveDiscussion(int id)
    {
        var result = await discussionService.SoftDeleteDiscussion(id);
        SetNotification(result);
        return RedirectToAction("ListOfDiscussions");
    }
}
