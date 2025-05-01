using ITSM.Enums;
using ITSM.Repositories.Discussion;
using ITSM.Repositories.TicketCategory;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class DiscussionController(
    ITicketCategoryRepository categoryRepository,
    IDiscussionRepository discussionRepository,
    IUserManagementRepository userManagementRepository) : Controller
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
        var listOfTreads = await discussionRepository.GetAllDiscussions(Status.Open);
        if (search != null)
        {
            listOfTreads = await discussionRepository.SearchDiscussion(Status.Open, search);
        }

        return View(listOfTreads);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDiscussion()
    {
        var categories = await categoryRepository.GetCategorySelectListAsync();


        var model = new DiscussionCreateViewModel
        {
            Categories = categories
        };


        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscussion(DiscussionCreateViewModel viewModel)
    {
        viewModel.Categories = await categoryRepository.GetCategorySelectListAsync();
        var userId = await userManagementRepository.GetCurrentUserAsync(User);
        if (userId != null)
        {
            var result = await discussionRepository.CreateDiscussion(viewModel, userId.Id);
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
        var discussionThread = await discussionRepository.GetDiscussionByIdWithMessages(id);
        return View(discussionThread);
    }


    [HttpPost]
    public async Task<IActionResult> AddMessage(int id, string messageContent)
    {
        var userId = await userManagementRepository.GetCurrentUserAsync(User);
        if (userId != null)
        {
            var result = await discussionRepository.AddMessage(userId.Id, id, messageContent);
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
        var userId = await userManagementRepository.GetCurrentUserAsync(User);
        var list = await discussionRepository.GetUserDiscussions(userId.Id);
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> ResolveDiscussion(int id)
    {
        var userId = await userManagementRepository.GetCurrentUserAsync(User);
        if (userId != null)
        {
            var result = await discussionRepository.ResolveDiscussion(id, userId.Id);
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
        var listOfTreads = await discussionRepository.GetAllDiscussions(Status.Resolved);
        if (search != null)
        {
            listOfTreads = await discussionRepository.SearchDiscussion(Status.Resolved, search);
        }

        return View(listOfTreads);
    }

    [HttpGet]
    public async Task<IActionResult> ArchiveViewDiscussion(int id)
    {
        var discussionThread = await discussionRepository.GetDiscussionByIdWithMessages(id);

        return View(discussionThread);
    }
}