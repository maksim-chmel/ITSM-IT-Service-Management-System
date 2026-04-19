using ITSM.Models;

namespace ITSM.ViewModels.Manage;

public class TicketDetailsViewModel
{
    public int Id { get; set; } 
    public Ticket Ticket { get; set; } = new();
    public List<TicketHistory> TicketHistory { get; set; } = new();
    public string? AuthorName { get; set; }
    public List<ITSM.Models.Discussion> Discussions { get; set; } = new();
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableArticles { get; set; } = new();
}