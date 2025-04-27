using ITSM.Repositories.KnowledgeBase;
using ITSM.Repositories.TicketCategory;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class KnowledgeBaseController(
    ITicketCategoryRepository categoryRepository,
    IKnowledgeBaseRepository knowledgeBaseRepository,
    IUserManagementRepository userRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> KnowledgeBaseByCategories()
    {
        var articles = await knowledgeBaseRepository.GetAllArticlesByCategory();
        return View(articles);
    }
    [HttpGet]
    public async Task<IActionResult> ViewArticle(int id)
    {
        var articles = await knowledgeBaseRepository.GetArticleById(id);
        return View(articles);
    }
    [HttpGet]
    public async Task<IActionResult> AllAuthorArticles()
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);

        var articles = await knowledgeBaseRepository.GetAllAuthorArticles(currentUser.Id);
        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> CreateArticle()
    {
        ViewBag.Categories = await categoryRepository.GetCategorySelectList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateArticle(CreateKnowArtViewModel viewModel)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            await knowledgeBaseRepository.CreateArticle(currentUser.Id, viewModel);
            TempData["SuccessMessage"] = "Article created.";
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("AllAuthorArticles");
    }
    [HttpPost]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            await knowledgeBaseRepository.DeleteArticle(id,currentUser.Id);
            TempData["SuccessMessage"] = "Article delete.";
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
        var article = await knowledgeBaseRepository.GetArticleById(id);
        var viewmodel = new EditKnowBaseViewModel
        {
            Id = article.Id,
            Content = article.Content,
            Article = article.Article,
            CategoryId = article.CategoryId
        };
        ViewBag.Categories = await categoryRepository.GetCategorySelectList(); 
        return View(viewmodel);


    }

    [HttpPost]
    public async Task<IActionResult> EditArticle(EditKnowBaseViewModel viewModel)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        await knowledgeBaseRepository.UpdateArticle(currentUser.Id, viewModel);
        return RedirectToAction("AllAuthorArticles");
    }
}