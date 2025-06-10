using ITSM.Enums;
using ITSM.Models;
using ITSM.ViewModels.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[AllowAnonymous]
public class AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                TempData["ErrorMessage"] = "A user with this email already exists.";

                return RedirectToAction("Register");
            }

            var user = new User { UserName = model.UserName, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, nameof(UserRoles.User));
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error.Description;
            }
        }

        return View(model);
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null && !user.IsDeleted)
            {
                var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = "Invalid username or password.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Please fill in all fields correctly.";
        }

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Landing");
    }
}