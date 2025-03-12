using ITSM.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class EmployeeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
}