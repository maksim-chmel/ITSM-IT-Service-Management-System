using System.Diagnostics;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc;


namespace ITSM.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

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