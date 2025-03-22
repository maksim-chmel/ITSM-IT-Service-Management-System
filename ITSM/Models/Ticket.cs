using ITSM.Enums;

namespace ITSM.Models;

public class Ticket
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public string? FixDescription { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string? AuthorId { get; set; }
    public User? Author { get; set; }
    public string? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public List<TicketHistory> TicketHistory { get; set; } = new();
     public int? CategoryId { get; set; }
    public TicketCategory? Category { get; set; }
}