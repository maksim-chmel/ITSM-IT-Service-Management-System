using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[AllowAnonymous]
public class LandingController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}