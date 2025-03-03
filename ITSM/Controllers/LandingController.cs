using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class LandingController : Controller
{
   
    public IActionResult Index()
    {
        return View();
    }
}