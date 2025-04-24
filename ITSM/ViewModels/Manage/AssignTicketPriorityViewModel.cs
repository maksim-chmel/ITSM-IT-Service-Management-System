using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Manage;

public class AssignTicketPriorityViewModel
{
    public Ticket Ticket { get; set; }
    public TicketPriority SelectedPriority { get; set; }  
    public List<SelectListItem> Priorities { get; set; } 
}