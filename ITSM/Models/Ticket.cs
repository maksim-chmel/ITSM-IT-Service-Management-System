using ITSM.Enums;

namespace ITSM.Models;

public class Ticket
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ClosedAt { get; set; }
    public string? AssignedTo { get; set; } // кому
    public string? IpAdrr { get; set; }
    public string? Type { get; set; }
    public string? ContactNumber { get; set; }
    public string? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    
    public string? FixDescription { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}