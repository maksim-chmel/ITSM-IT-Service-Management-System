using ITSM.Models;
using ITSM.Services.UserProfile;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize]
public class UserProfileController(IUserProfileService userProfileService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await userProfileService.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = await userProfileService.GetUserProfileAsync(user);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            NotifyError("Please correct the errors in the form.");
            return View(model);
        }

        var user = await userProfileService.GetUserAsync(User);
        if (user == null)
        {
            NotifyError("User not found.");
            return NotFound();
        }

        var profileResult = await userProfileService.UpdateUserProfileAsync(user, model);
        if (!profileResult.IsSuccess)
        {
            SetNotification(profileResult);
            return View(model);
        }

        if (IsPasswordChangeRequested(model))
        {
            var passwordResult = await userProfileService.ChangeUserPasswordAsync(user, model.CurrentPassword!, model.NewPassword!);
            if (!passwordResult.IsSuccess)
            {
                SetNotification(passwordResult);
                return View(model);
            }
        }

        await userProfileService.RefreshUserSignInAsync(user);
        NotifySuccess("Profile updated successfully.");
        return RedirectToAction(nameof(EditProfile));
    }

    private bool IsPasswordChangeRequested(EditUserViewModel model)
    {
        return
            !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            !string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmPassword);
    }
}