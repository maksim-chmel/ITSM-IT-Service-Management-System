using ITSM.Enums;
using ITSM.Repositories.Discussion;
using ITSM.Repositories.TicketCategory;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Create;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class DiscussionController(
    ITicketCategoryRepository categoryRepository,
    IDiscussionRepository discussionRepository,
    IUserManagementRepository userManagementRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ListOfDiscussions(string? search)
    {
        var listOfTreads = await discussionRepository.GetAllDiscussions(Status.Open);
        if (search != null)
        {
            listOfTreads = await discussionRepository.SearchDiscussion(Status.Open,search);
        }

        return View(listOfTreads);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDiscussion()
    {
        var categories = await categoryRepository.GetCategorySelectList();
        

        var model = new DiscussionCreateViewModel
        {
            Categories = categories
        };


        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscussion(DiscussionCreateViewModel viewModel)
    {
        viewModel.Categories = await categoryRepository.GetCategorySelectList();
        var userId = await userManagementRepository.GetCurrentUserAsync(User);
        await discussionRepository.CreateDiscussion(viewModel, userId.Id);
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
        await discussionRepository.AddMessage(userId.Id, id, messageContent);

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
        await discussionRepository.ResolveDiscussion(id, userId.Id);
        return RedirectToAction("ManageDiscussions");
    }

    [HttpGet]
    public async Task<IActionResult> ArchiveOfDiscussions(string? search)
    {
        var listOfTreads = await discussionRepository.GetAllDiscussions(Status.Resolved);
        if (search != null)
        {
            listOfTreads = await discussionRepository.SearchDiscussion(Status.Resolved,search);
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