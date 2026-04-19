using System.ComponentModel.DataAnnotations;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Manage;

public class AssignTicketViewModel
{
    public Ticket Ticket { get; set; } = new();
    [Required(ErrorMessage = "Please select a user.")]
    public string SelectedUserId { get; set; } = string.Empty;
    public List<SelectListItem> Users { get; set; } = new();
}
