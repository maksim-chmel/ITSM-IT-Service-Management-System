using ITSM.Models;
using ITSM.Services.UserProfile;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class UserProfileController(IUserProfileService userProfileService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await userProfileService.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = await userProfileService.GetUserProfileAsync(user);
        ViewBag.StatusMessage = TempData["StatusMessage"];
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetTempDataError("Please correct the errors in the form.");

            return View(model);
        }

        var user = await userProfileService.GetUserAsync(User);
        if (user == null)
        {
            SetTempDataError("User not found.");
            return NotFound();
        }

        if (!await UpdateUserProfileAsync(user, model))
        {
            return View(model);
        }

        if (IsPasswordChangeRequested(model))
        {
            if (!await ChangePasswordAsync(user, model))
            {
                return View(model);
            }
        }

        await userProfileService.RefreshUserSignInAsync(user);
        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(EditProfile));
    }

    private void SetTempDataError(string message)
    {
        TempData["StatusMessage"] = "Error: " + message;
    }

    private async Task<bool> UpdateUserProfileAsync(User user, EditUserViewModel model)
    {
        var updateResult = await userProfileService.UpdateUserProfileAsync(user, model);
        if (updateResult.Succeeded) return true;
        foreach (var error in updateResult.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        SetTempDataError("Failed to update the profile.");
        return false;
    }

    private bool IsPasswordChangeRequested(EditUserViewModel model)
    {
        return
            !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            !string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmPassword);
    }

    private async Task<bool> ChangePasswordAsync(User user, EditUserViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            string.IsNullOrWhiteSpace(model.NewPassword) ||
            string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            ModelState.AddModelError(string.Empty, "Please fill in all password fields.");
            SetTempDataError("Please fill in all password fields.");

            return false;
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "The new password and confirmation do not match.");
            SetTempDataError("The new password and confirmation do not match.");

            return false;
        }

        var passwordResult =
            await userProfileService.ChangeUserPasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (passwordResult.Succeeded) return true;
        foreach (var error in passwordResult.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        SetTempDataError("Failed to change the password.");
        return false;
    }
}