using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels;

public class AssignTicketViewModel
{
    public Ticket Ticket { get; set; }
    public TicketPriority Priority { get; set; }  
    public List<SelectListItem> Users { get; set; }
    public List<SelectListItem> Priorities { get; set; }  
}
