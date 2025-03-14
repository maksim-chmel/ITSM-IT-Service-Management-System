using ITSM.Models;

namespace ITSM.ViewModels;

public class TicketDetailsViewModel
{
    public int Id { get; set; } 
    public Ticket Ticket { get; set; } 
    public List<TicketHistory> TicketHistory { get; set; } 
}