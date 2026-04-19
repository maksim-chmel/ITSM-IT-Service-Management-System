using System.ComponentModel.DataAnnotations;
using ITSM.Enums;
using ITSM.Services.Archive;

namespace ITSM.Models;
public class Discussion : ISoftDeletable
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    [Required]
    public User Author { get; set; } = null!;

    public Status Status { get; set; } = Status.Open;

    public List<DiscussionMessage> Messages { get; set; } = new();
    
    [Required]
    public int CategoryId { get; set; }

    [Required]
    public TicketCategory Category { get; set; } = null!;

    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public bool IsDeleted { get; set; }= false;
}
