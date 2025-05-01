using ITSM.Repositories.UserProfile;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;
[Authorize]
public class UserProfileController(IUserProfileRepository userProfileRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await userProfileRepository.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = await userProfileRepository.GetUserProfileAsync(user);
        ViewBag.StatusMessage = TempData["StatusMessage"];
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userProfileRepository.GetUserAsync(User);
        if (user == null) return NotFound();

        var updateResult = await userProfileRepository.UpdateUserProfileAsync(user, model);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        var isPasswordChangeRequested =
            !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            !string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmPassword);

        if (isPasswordChangeRequested)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Please fill out all password fields.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
                return View(model);
            }

            var passwordResult = await userProfileRepository.ChangeUserPasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
        }

        await userProfileRepository.RefreshUserSignInAsync(user);
        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(EditProfile));
    }
}
