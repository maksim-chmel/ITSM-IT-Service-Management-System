using System.ComponentModel.DataAnnotations;
using ITSM.Enums;

namespace ITSM.Models;
public class Discussion
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Если обсуждение закрывается — поле обязательно
    public DateTime? ClosedAt { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    [Required]
    public User Author { get; set; } = null!;

    public Status Status { get; set; } = Status.Open;

    public List<DiscussionMessage> Messages { get; set; } = new();

    // Категория обязательна, если каждое обсуждение должно быть привязано к категории.
    [Required]
    public int CategoryId { get; set; }

    [Required]
    public TicketCategory Category { get; set; } = null!;

    public bool IsDeleted { get; set; }= false;
}
