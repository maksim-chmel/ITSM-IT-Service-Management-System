using ITSM.Data;
using ITSM.Enums;
using ITSM.Authorization;
using ITSM.Services.Archive;
using ITSM.Services.Discussion;
using ITSM.Services.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Controllers;
[Authorize(Roles = "Admin,Coordinator,Technician")]
public class ArchiveController(
    IArchiveService archiveService,
    IDiscussionService discussionService,
    ITicketService ticketService,
    IAuthorizationService authorizationService,
    DBaseContext context) : BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ArchivedDiscussions(string? search)
    {
        var discussions = await archiveService.GetArchivedDiscussionsAsync(search);
        return View("ArchivedDiscussions", discussions);
    }

    [HttpGet]
    public async Task<IActionResult> ViewArchivedDiscussion(int id)
    {
        var discussion = await discussionService.GetDiscussionByIdWithMessages(id);
        if (discussion == null) return NotFound();
        return View("ViewArchivedDiscussion", discussion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreDiscussion(int id)
    {
        var result = await discussionService.RestoreDiscussion(id);
        SetNotification(result);
        return RedirectToAction("ArchivedDiscussions");
    }

    [HttpGet]
    public async Task<IActionResult> DeletedArticles()
    {
        var articles = await archiveService.GetDeletedArticlesAsync();
        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> DeletedTickets()
    {
        var tickets = await archiveService.GetDeletedTicketsAsync();
        return View(tickets);
    }

    [HttpGet]
    public async Task<IActionResult> DeletedCategories()
    {
        var categories = await archiveService.GetDeletedCategoriesAsync();
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> DeletedUsers()
    {
        var users = await archiveService.GetDeletedUsersAsync();
        return View(users);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreArticles(List<int> selectedArticleIds)
    {
        if (selectedArticleIds == null || !selectedArticleIds.Any())
            return RedirectToAction("DeletedArticles");

        var result = await archiveService.RestoreEntitiesAsync(context.KnowledgeBaseArticles,selectedArticleIds);
        SetNotification(result);
        return RedirectToAction("DeletedArticles");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreTickets(List<int> selectedTicketIds)
    {
        if (selectedTicketIds == null || !selectedTicketIds.Any())
            return RedirectToAction("DeletedTickets");

        var actor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var restored = 0;

        foreach (var ticketId in selectedTicketIds.Distinct())
        {
            var ticket = await context.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) continue;

            var auth = await authorizationService.AuthorizeAsync(User, ticket, new TicketRequirement(TicketOperations.Restore));
            if (!auth.Succeeded) continue;

            var res = await ticketService.RestoreTicketAsync(ticketId, actor);
            if (res.IsSuccess) restored++;
        }

        if (restored > 0)
            NotifySuccess($"{restored} tickets restored successfully.");
        else
            NotifyError("No tickets were restored.");

        return RedirectToAction("DeletedTickets");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreTaxonomy(List<int> selectedCategoryIds, List<int> selectedSubCategoryIds)
    {
        if ((selectedCategoryIds == null || !selectedCategoryIds.Any()) && (selectedSubCategoryIds == null || !selectedSubCategoryIds.Any()))
        {
            NotifyError("No items selected for restoration.");
            return RedirectToAction("DeletedCategories");
        }

        if (selectedCategoryIds != null && selectedCategoryIds.Any())
        {
            var categoriesToRestore = await context.TicketCategories
                .IgnoreQueryFilters()
                .Include(c => c.SubCategories)
                .Where(c => selectedCategoryIds.Contains(c.Id))
                .ToListAsync();

            foreach(var category in categoriesToRestore)
            {
                category.IsDeleted = false;
                foreach(var subCategory in category.SubCategories)
                {
                    subCategory.IsDeleted = false;
                }
            }
            await context.SaveChangesAsync();
            NotifySuccess($"{categoriesToRestore.Count} categories and their subcategories restored.");
        }

        if (selectedSubCategoryIds != null && selectedSubCategoryIds.Any())
        {
            var subCategoryResult = await archiveService.RestoreEntitiesAsync(context.TicketSubCategories, selectedSubCategoryIds);
            SetNotification(subCategoryResult);
        }

        return RedirectToAction("DeletedCategories");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreUsers(List<string> selectedUserIds)
    {
        if (selectedUserIds == null || !selectedUserIds.Any())
            return RedirectToAction("DeletedUsers");

        var result = await archiveService.RestoreUsersAsync(selectedUserIds);
        SetNotification(result);
        return RedirectToAction("DeletedUsers");
    }
}
