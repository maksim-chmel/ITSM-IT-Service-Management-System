using ITSM.Services.KnowledgeBase;
using ITSM.Services.TicketCategory;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Create;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class KnowledgeBaseController(
    ITicketCategoryService categoryService,
    IKnowledgeBaseService knowledgeBaseService,
    IUserManagementService userService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> KnowledgeBaseByCategories()
    {
        var articles = await knowledgeBaseService.GetAllArticlesByCategory();
        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> ViewArticle(int id)
    {
        var articles = await knowledgeBaseService.GetArticleById(id);
        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> AllAuthorArticles()
    {
        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser == null)
        {
            NotifyError("User not found.");
            return RedirectToAction("Login", "Auth");
        }

        var articles = await knowledgeBaseService.GetAllAuthorArticles(currentUser.Id);
        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> CreateArticle()
    {
        ViewBag.Categories = await categoryService.GetCategorySelectListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArticle(CreateKnowArtViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetCategorySelectListAsync();
            return View(viewModel);
        }

        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser == null)
        {
            NotifyError("User not found.");
            return RedirectToAction("AllAuthorArticles");
        }

        var result = await knowledgeBaseService.CreateArticle(currentUser.Id, viewModel);
        SetNotification(result);

        return RedirectToAction("AllAuthorArticles");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveArticle(int id)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            var result = await knowledgeBaseService.ArchiveArticle(id, currentUser.Id);
            SetNotification(result);
        }
        else
        {
            NotifyError("User not found.");
        }

        return RedirectToAction("AllAuthorArticles");
    }

    [HttpGet]
    public async Task<IActionResult> EditArticle(int id)
    {
        var article = await knowledgeBaseService.GetArticleById(id);
        if (article == null) return NotFound();

        var viewmodel = new EditKnowBaseViewModel
        {
            Id = article.Id,
            Content = article.Content,
            Article = article.Article,
            CategoryId = article.CategoryId
        };
        ViewBag.Categories = await categoryService.GetCategorySelectListAsync();
        return View(viewmodel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditArticle(EditKnowBaseViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetCategorySelectListAsync();
            return View(viewModel);
        }

        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser == null)
        {
            NotifyError("User not found.");
            return RedirectToAction("AllAuthorArticles");
        }

        var result = await knowledgeBaseService.UpdateArticle(currentUser.Id, viewModel);
        SetNotification(result);

        return RedirectToAction("AllAuthorArticles");
    }
}
