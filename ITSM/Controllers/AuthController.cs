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
                TempData["ErrorMessage"] = "Пользователь с таким email уже существует.";
                return RedirectToAction("Register");
            }

            var user = new User { UserName = model.UserName, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
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

                TempData["ErrorMessage"] = "Неверный пароль или имя пользователя.";
            }
            else
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Заполните все поля корректно.";
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