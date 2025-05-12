using ITSM.Repositories.Archive;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class ArchiveController(IArchiveService archiveService) : Controller
{
    public IActionResult Index()
    {
        return View();
    }


    public async Task<IActionResult> DeletedArticles()
    {
        var articles = await archiveService.GetDeletedArticlesAsync();
        return View(articles);
    }


    public async Task<IActionResult> DeletedTickets()
    {
        var tickets = await archiveService.GetDeletedTicketsAsync();
        return View(tickets);
    }

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
    public async Task<IActionResult> RestoreArticles(List<int> selectedArticleIds)
    {
        if (selectedArticleIds == null || !selectedArticleIds.Any())
            return RedirectToAction("DeletedArticles");

        await archiveService.RestoreArticlesAsync(selectedArticleIds);
        return RedirectToAction("DeletedArticles");
    }


    [HttpPost]
    public async Task<IActionResult> RestoreTickets(List<int> selectedTicketIds)
    {
        if (selectedTicketIds == null || !selectedTicketIds.Any())
            return RedirectToAction("DeletedTickets");

        await archiveService.RestoreTicketsAsync(selectedTicketIds);
        return RedirectToAction("DeletedTickets");
    }

    [HttpPost]
    public async Task<IActionResult> RestoreCategories(List<int> selectedCategoryIds)
    {
        if (selectedCategoryIds == null || !selectedCategoryIds.Any())
            return RedirectToAction("DeletedCategories");

        await archiveService.RestoreCategoriesAsync(selectedCategoryIds);
        return RedirectToAction("DeletedCategories");
    }

    [HttpPost]
    public async Task<IActionResult> RestoreSubCategories(List<int> selectedSubCategoryIds)
    {
        if (selectedSubCategoryIds == null || !selectedSubCategoryIds.Any())
            return RedirectToAction("DeletedCategories");

        await archiveService.RestoreSubCategoriesAsync(selectedSubCategoryIds);
        return RedirectToAction("DeletedCategories");
    }

    [HttpPost]
    public async Task<IActionResult> RestoreUsers(List<string> selectedUserIds)
    {
        if (selectedUserIds == null || !selectedUserIds.Any())
            return RedirectToAction("DeletedUsers");

        await archiveService.RestoreUsersAsync(selectedUserIds);
        return RedirectToAction("DeletedUsers");
    }
}