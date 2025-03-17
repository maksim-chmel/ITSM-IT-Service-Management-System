using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class CoordinatorTicketController : Controller
{
   [HttpGet]
    public IActionResult TicketAssignment()
    {
        return View();
    }
}