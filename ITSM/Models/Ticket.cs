using ITSM.Enums;

namespace ITSM.Models;

public class Ticket
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; } = "Normal";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedTo { get; set; } // кому
    public string? IpAdrr { get; set; }
    public string? Type { get; set; }
    public string? ContactNumber { get; set; }
    public string? CreatedBy { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
}