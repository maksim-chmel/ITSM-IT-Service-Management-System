using ITSM.Repositories.Archive;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class ArchiveController(IArchiveRepository archiveRepository) : Controller
{
    public IActionResult Index()
    {
        return View();
    }


    public async Task<IActionResult> DeletedArticles()
    {
        var articles = await archiveRepository.GetDeletedArticlesAsync();
        return View(articles);
    }


    public async Task<IActionResult> DeletedTickets()
    {
        var tickets = await archiveRepository.GetDeletedTicketsAsync();
        return View(tickets);
    }

    public async Task<IActionResult> DeletedCategories()
    {
        var categories = await archiveRepository.GetDeletedCategoriesAsync();
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> DeletedUsers()
    {
        var users = await archiveRepository.GetDeletedUsersAsync();
        return View(users);
    }


    [HttpPost]
    public async Task<IActionResult> RestoreArticles(List<int> selectedArticleIds)
    {
        if (selectedArticleIds == null || !selectedArticleIds.Any())
            return RedirectToAction("DeletedArticles");

        await archiveRepository.RestoreArticlesAsync(selectedArticleIds);
        return RedirectToAction("DeletedArticles");
    }


    [HttpPost]
    public async Task<IActionResult> RestoreTickets(List<int> selectedTicketIds)
    {
        if (selectedTicketIds == null || !selectedTicketIds.Any())
            return RedirectToAction("DeletedTickets");

        await archiveRepository.RestoreTicketsAsync(selectedTicketIds);
        return RedirectToAction("DeletedTickets");
    }

    [HttpPost]
    public async Task<IActionResult> RestoreCategories(List<int> selectedCategoryIds)
    {
        if (selectedCategoryIds == null || !selectedCategoryIds.Any())
            return RedirectToAction("DeletedCategories");

        await archiveRepository.RestoreCategoriesAsync(selectedCategoryIds);
        return RedirectToAction("DeletedCategories");
    }

    [HttpPost]
    public async Task<IActionResult> RestoreSubCategories(List<int> selectedSubCategoryIds)
    {
        if (selectedSubCategoryIds == null || !selectedSubCategoryIds.Any())
            return RedirectToAction("DeletedCategories");

        await archiveRepository.RestoreSubCategoriesAsync(selectedSubCategoryIds);
        return RedirectToAction("DeletedCategories");
    }

    [HttpPost]
    public async Task<IActionResult> RestoreUsers(List<string> selectedUserIds)
    {
        if (selectedUserIds == null || !selectedUserIds.Any())
            return RedirectToAction("DeletedUsers");

        await archiveRepository.RestoreUsersAsync(selectedUserIds);
        return RedirectToAction("DeletedUsers");
    }
}