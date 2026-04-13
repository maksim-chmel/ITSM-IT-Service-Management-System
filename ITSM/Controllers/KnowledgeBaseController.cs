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
            TempData["ErrorMessage"] = "User not found.";
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
    public async Task<IActionResult> CreateArticle(CreateKnowArtViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetCategorySelectListAsync();
            return View(viewModel);
        }

        var currentUser = await userService.GetCurrentUserAsync(User);
        var result = currentUser != null && await knowledgeBaseService.CreateArticle(currentUser.Id, viewModel);

        SetTempDataMessage(result, "Article created successfully.", currentUser == null
            ? "User not found."
            : "Error while creating the article.");

        return RedirectToAction("AllAuthorArticles");
    }


    [HttpPost]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var currentUser = await userService.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            var result = await knowledgeBaseService.DeleteArticle(id, currentUser.Id);
            SetTempDataMessage(result, "Article deleted successfully.",
                "Error while deleting the article.");
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("AllAuthorArticles");
    }

    [HttpGet]
    public async Task<IActionResult> EditArticle(int id)
    {
        var article = await knowledgeBaseService.GetArticleById(id);
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
    public async Task<IActionResult> EditArticle(EditKnowBaseViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await categoryService.GetCategorySelectListAsync();
            return View(viewModel);
        }

        var currentUser = await userService.GetCurrentUserAsync(User);
        var result = currentUser != null && await knowledgeBaseService.UpdateArticle(currentUser.Id, viewModel);

        SetTempDataMessage(result, "Article updated successfully.", currentUser == null
            ? "User not found."
            : "Error while updating the article.");

        return RedirectToAction("AllAuthorArticles");
    }
}
