using ITSM.Enums;
using ITSM.Repositories.RoleManager;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = nameof(UserRoles.Admin))]
public class UserRolesController(IUserRolesRepository userRolesRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        var model = await userRolesRepository.GetUserRolesViewModel(userId);
        if (model == null)
            return NotFound("User not found");

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
    {
        var result = await userRolesRepository.UpdateUserRolesAsync(model.UserId, model.Roles);
        if (!result)
            return NotFound("User not found");

        return RedirectToAction("UsersList", "UserManagement");
    }
}

