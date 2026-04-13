using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;
public class DiscussionMessage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Message content is required.")]
    [StringLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters.")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int DiscussionId { get; set; }

    [Required]
    public Discussion Discussion { get; set; } = null!;

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    [Required]
    public User Author { get; set; } = null!;
}
