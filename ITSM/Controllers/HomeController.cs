using ITSM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ITSM.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["UserName"] = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Error(string message = "Произошла ошибка")
    {
        var model = new ErrorViewModel { ErrorMessage = message };
        return View(model);
    }
}