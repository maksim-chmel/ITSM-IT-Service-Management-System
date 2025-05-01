using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;
public class DiscussionMessage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Message content is required.")]
    [StringLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters.")]
    public string Content { get; set; } = string.Empty;  // Присваиваем значение по умолчанию

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]  // Обсуждение обязательно для каждого сообщения
    public int DiscussionId { get; set; }

    [Required]  // Каждое сообщение обязательно должно быть привязано к обсуждению
    public Discussion Discussion { get; set; } = null!;

    [Required]  // Автор обязательно для каждого сообщения
    public string AuthorId { get; set; } = string.Empty;

    [Required]  // Автор обязательно для каждого сообщения
    public User Author { get; set; } = null!;
}

