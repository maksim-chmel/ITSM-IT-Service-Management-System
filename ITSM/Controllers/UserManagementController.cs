using ITSM.Repositories;
using ITSM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController(IUserManagementRepository userRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> UsersList()
    {
        var list = await userRepository.GetAllUsersToList();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await userRepository.DeleteUserById(id);
        return RedirectToAction("UsersList");
    }

    [HttpGet]
    public async Task<IActionResult> UserEditor(string id)
    {
        var user = await userRepository.GetUserById(id);

        if (user == null) return NotFound();
        var editModel = new EditUserViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };

        return View(editModel);
    }

    [HttpPost]
    public async Task<IActionResult> UserEditor(string id, EditUserViewModel editModel)
    {
        if (!ModelState.IsValid) return View(editModel);
        await userRepository.EditUser(id, editModel);
        return RedirectToAction("UsersList");
    }
}