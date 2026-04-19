using ITSM.Enums;
using ITSM.Services.RoleManager;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class UserRolesController(IUserRolesService userRolesService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        var model = await userRolesService.GetUserRolesViewModel(userId);
        if (model == null)
            return NotFound("User not found");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
    {
        var result = await userRolesService.UpdateUserRolesAsync(model.UserId, model.Roles);
        SetNotification(result);
        return RedirectToAction("UsersList", "UserManagement");
    }
}