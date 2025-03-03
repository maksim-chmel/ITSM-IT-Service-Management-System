using ITSM.Enums;


namespace ITSM.ViewModels;

public class TicketViewModel
{
    public int Id { get; set; } 
    public string? Title { get; set; } 
    public string? Description { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    
}