using ITSM.Enums;
using ITSM.Repositories.Discussion;
using ITSM.Repositories.TicketCategory;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class DiscussionController(
    ITicketCategoryService categoryService,
    IDiscussionService discussionService,
    IUserManagementService userManagementService) : Controller
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
    public async Task<IActionResult> CreateDiscussion()
    {
        var categories = await categoryService.GetCategorySelectListAsync();


        var model = new DiscussionCreateViewModel
        {
            Categories = categories
        };


        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscussion(DiscussionCreateViewModel viewModel)
    {
        viewModel.Categories = await categoryService.GetCategorySelectListAsync();
        var userId = await userManagementService.GetCurrentUserAsync(User);
        if (userId != null)
        {
            var result = await discussionService.CreateDiscussion(viewModel, userId.Id);
            SetTempDataMessage(result, "Обсуждение успешно создан.", "Ошибка при создании обсуждения.");
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("ListOfDiscussions");
    }

    [HttpGet]
    public async Task<IActionResult> ViewDiscussion(int id)
    {
        var discussionThread = await discussionService.GetDiscussionByIdWithMessages(id);
        return View(discussionThread);
    }


    [HttpPost]
    public async Task<IActionResult> AddMessage(int id, string messageContent)
    {
        var userId = await userManagementService.GetCurrentUserAsync(User);
        if (userId != null)
        {
            var result = await discussionService.AddMessage(userId.Id, id, messageContent);
            SetTempDataMessage(result, "Комментарий успешно создан.", "Ошибка при создании комментария.");
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("ViewDiscussion", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ManageDiscussions()
    {
        var userId = await userManagementService.GetCurrentUserAsync(User);
        var list = await discussionService.GetUserDiscussions(userId.Id);
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> ResolveDiscussion(int id)
    {
        var userId = await userManagementService.GetCurrentUserAsync(User);
        if (userId != null)
        {
            var result = await discussionService.ResolveDiscussion(id, userId.Id);
            SetTempDataMessage(result, "Дискуссия успешно решена.", "Ошибка при решении  дискуссии.");
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }

        return RedirectToAction("ManageDiscussions");
    }

    [HttpGet]
    public async Task<IActionResult> ArchiveOfDiscussions(string? search)
    {
        var listOfTreads = await discussionService.GetAllDiscussions(Status.Resolved);
        if (search != null)
        {
            listOfTreads = await discussionService.SearchDiscussion(Status.Resolved, search);
        }

        return View(listOfTreads);
    }

    [HttpGet]
    public async Task<IActionResult> ArchiveViewDiscussion(int id)
    {
        var discussionThread = await discussionService.GetDiscussionByIdWithMessages(id);

        return View(discussionThread);
    }
}