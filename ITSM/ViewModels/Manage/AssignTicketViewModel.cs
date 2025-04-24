using ITSM.Enums;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Manage;

public class AssignTicketViewModel
{
    public Ticket Ticket { get; set; }
    public string SelectedUserId { get; set; } 
    public List<SelectListItem> Users { get; set; } 
}
