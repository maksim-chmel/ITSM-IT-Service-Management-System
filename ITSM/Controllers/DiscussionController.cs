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
    public async Task<IActionResult> ListOfDiscussions()
    {
        var listOfTreads = await discussionRepository.GetAllDiscussions();
        return View(listOfTreads);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDiscussion()
    {
        var categories = await categoryRepository.GetCategorySelectList();


        var model = new DiscussionCreateViewModel()
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
    
        // Логирование для проверки
        if (discussionThread != null)
        {
            Console.WriteLine($"Discussion found: Id = {discussionThread.Id}, Title = {discussionThread.Title}, MessagesCount = {discussionThread.Messages.Count}");
        }

        return View(discussionThread);
    }


    [HttpPost]
    public async Task<IActionResult> AddMessage(int id, string messageContent)
    {
        var userId = await userManagementRepository.GetCurrentUserAsync(User);
        await discussionRepository.AddMessage(userId.Id, id, messageContent);

        return RedirectToAction("ViewDiscussion", new { id });
    }
}