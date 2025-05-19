using ITSM.Enums;
using ITSM.Services.Qualification;
using ITSM.Services.UserManagment;
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
    public async Task<IActionResult> DeleteUser(string id)
    {
     var   result =await userService.SoftDeleteUserById(id);
     SetTempDataMessage(result, "User deleted successfully.", "Error deleting the user.");
        return RedirectToAction("UsersList");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await userService.CreateEditUserViewModel(id);

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(string id, EditUserViewModel editModel)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(editModel);
        }
        try
        {
            await userService.EditUser(id, editModel);
            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction("UsersList");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error occurred while updating the user.";
            return View(editModel);
        }

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
        var result = await qualification.AssignCategoriesToUserAsync(model);
        SetTempDataMessage(result, "User assigned successfully.", "Error assigning the user.");
        return RedirectToAction("UsersList");
    }
}