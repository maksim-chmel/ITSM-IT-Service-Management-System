using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;
public class KnowledgeBaseArticle
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Article title is required.")]
    [StringLength(100, ErrorMessage = "Article title cannot exceed 100 characters.")]
    public string Article { get; set; } = string.Empty;  // Присваиваем значение по умолчанию

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;  // Присваиваем значение по умолчанию

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // По умолчанию создаём время в UTC

    [Required]  // Обязательно, так как каждый артикул должен иметь автора
    public string AuthorId { get; set; } = string.Empty;

    [Required]  // Обязательно, так как каждый артикул должен иметь привязку к автору
    public User Author { get; set; } = null!;

    [Required]  // Обязательно, так как каждый артикул должен быть привязан к категории
    public int CategoryId { get; set; }

    [Required]  // Обязательно, так как артикул не может существовать без категории
    public TicketCategory Category { get; set; } = null!;
    public bool IsDeleted { get; set; }= false;
}
