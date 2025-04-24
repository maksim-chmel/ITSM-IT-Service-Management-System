using ITSM.Enums;

namespace ITSM.Models;

public class Discussion
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public string AuthorId { get; set; }
    public User Author { get; set; }
    public Status Status { get; set; } = Status.Open;
    public List<DiscussionMessage> Messages { get; set; } = new();
    public int? CategoryId { get; set; }
    public TicketCategory Category { get; set; }
}