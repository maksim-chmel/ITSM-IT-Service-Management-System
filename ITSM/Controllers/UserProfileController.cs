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
            !string.IsNullOrWhiteSpace(model.CurrentPassword) &&
            !string.IsNullOrWhiteSpace(model.NewPassword) &&
            !string.IsNullOrWhiteSpace(model.ConfirmPassword);

        if (isPasswordChangeRequested)
        {
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
