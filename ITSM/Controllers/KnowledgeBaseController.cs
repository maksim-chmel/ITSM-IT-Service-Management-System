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
    private void SetTempDataMessage(bool isSuccess, string successMessage, string errorMessage)
    {
        if (isSuccess)
        {
            TempData["SuccessMessage"] = successMessage;
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage;
        }
    }

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
        ViewBag.Categories = await categoryRepository.GetCategorySelectListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateArticle(CreateKnowArtViewModel viewModel)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            var result = await knowledgeBaseRepository.CreateArticle(currentUser.Id, viewModel);
            SetTempDataMessage(result, "Статья успешно создан.", "Ошибка при создании статьи.");
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
            var result = await knowledgeBaseRepository.DeleteArticle(id, currentUser.Id);
            SetTempDataMessage(result, "Cтатья успешно удалена.",
                "Ошибка при удалении статьи.");
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
        ViewBag.Categories = await categoryRepository.GetCategorySelectListAsync();
        return View(viewmodel);
    }

    [HttpPost]
    public async Task<IActionResult> EditArticle(EditKnowBaseViewModel viewModel)
    {
        var currentUser = await userRepository.GetCurrentUserAsync(User);
        if (currentUser != null)
        {
            var result = await knowledgeBaseRepository.UpdateArticle(currentUser.Id, viewModel);
            SetTempDataMessage(result, "Cтатья успешно Обновлена.",
                "Ошибка при обновлении статьи.");
        }

        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("AllAuthorArticles");
    }
}