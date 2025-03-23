using ITSM.Models;
using ITSM.Repositories;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = "Admin")]
public class UserRolesController(IUserRolesRepository userRolesRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Manage(string userId)
    {
        var model = await userRolesRepository.GetUserRolesViewModel(userId);
        if (model == null)
            return NotFound("User not found");

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Manage(ManageUserRolesViewModel model)
    {
        var result = await userRolesRepository.UpdateUserRolesAsync(model.UserId, model.Roles);
        if (!result)
            return NotFound("User not found");

        return RedirectToAction("Manage", "UserRoles", new { userId = model.UserId });
    }
}

