using ITSM.Enums;
using ITSM.Services.Qualification;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class UserManagementController(IUserManagementService userService,IQualificationService qualification) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> UsersList(string? search)
    {
        var list = await userService.GetAllUsersToList();
        if (search != null)
        {
            list = await userService.SearchUser(search);
        }

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
     var   result =await userService.SoftDeleteUserById(id);
     SetNotification(result);
        return RedirectToAction("UsersList");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await userService.CreateEditUserViewModel(id);

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string id, EditUserViewModel editModel)
    {
        if (!ModelState.IsValid)
        {
            NotifyError("Please correct the errors in the form.");
            return View(editModel);
        }
        
        var result = await userService.EditUser(id, editModel);
        SetNotification(result);
        
        if (result.IsSuccess)
            return RedirectToAction("UsersList");
            
        return View(editModel);
    }


    [HttpGet]
    public async Task<IActionResult> AssignCategoryToUser(string userId)
    {
        var model = await qualification.GetAssignCategoryViewModelAsync(userId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCategoryToUser(AssignCategoryToUserViewModel model)
    {
        var result = await qualification.AssignCategoriesToUserAsync(model);
        SetNotification(result);
        return RedirectToAction("UsersList");
    }
}