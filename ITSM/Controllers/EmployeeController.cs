using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

[Authorize(Roles = "User")]
public class EmployeeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}