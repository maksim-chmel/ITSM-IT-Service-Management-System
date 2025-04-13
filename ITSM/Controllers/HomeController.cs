using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ITSM.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["UserName"] = User.Identity is { IsAuthenticated: true } ? User.Identity.Name : "Guest";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Error()
    {
        var errorMessage = TempData["ErrorMessage"] as string;
        ViewData["ErrorMessage"] = errorMessage;

        return View();
    }
    //Test ERROR
    [HttpGet]
    public IActionResult ThrowError()
    {
        throw new Exception("This is a test error.");
    }

}