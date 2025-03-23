using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers;

public class TicketAssignmentController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}