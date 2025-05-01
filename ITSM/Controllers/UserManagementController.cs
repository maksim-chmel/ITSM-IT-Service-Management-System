using ITSM.Enums;
using ITSM.Repositories.Qualification;
using ITSM.Repositories.UserManagment;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class UserManagementController(IUserManagementRepository userRepository,IQualificationRepository qualification) : Controller
{
    [HttpGet]
    public async Task<IActionResult> UsersList(string? search)
    {
        var list = await userRepository.GetAllUsersToList();
        if (search != null)
        {
            list = await userRepository.SearchUser(search);
        }

        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await userRepository.SoftDeleteUserById(id);

        return RedirectToAction("UsersList");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await userRepository.CreateEditUserViewModel(id);

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(string id, EditUserViewModel editModel)
    {
        if (!ModelState.IsValid) return View(editModel);

        await userRepository.EditUser(id, editModel);

        return RedirectToAction("UsersList");
    }

    [HttpGet]
    public async Task<IActionResult> AssignCategoryToUser(string userId)
    {
        var model = await qualification.GetAssignCategoryViewModelAsync(userId);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AssignCategoryToUser(AssignCategoryToUserViewModel model)
    {
        var success = await qualification.AssignCategoriesToUserAsync(model);
        if (!success)
            return NotFound();

        return RedirectToAction("UsersList");
    }
}