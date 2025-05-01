using System.ComponentModel.DataAnnotations;
using ITSM.Enums;

namespace ITSM.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty; // Присваиваем значение по умолчанию (пустая строка)

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty; // Присваиваем значение по умолчанию (пустая строка)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public string? FixDescription { get; set; } // Nullable, так как не всегда обязательно

    [Required] // Статус обязательное поле
    public Status Status { get; set; } = Status.New;

    [Required] // Приоритет обязательное поле
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public string? CancelReason { get; set; } // Nullable, так как не всегда обязательно

    [Required]  // Author обязателен, так как тикет всегда имеет автора
    public string AuthorId { get; set; } = string.Empty;

    [Required]  // Author обязательно привязан
    public User Author { get; set; } = null!;

    public string? AssignedUserId { get; set; } // Nullable, так как назначение не обязательно
    public User? AssignedUser { get; set; } // Nullable, так как может не быть назначенного пользователя

    public List<TicketHistory> TicketHistory { get; set; } = new();

    [Required]  // Обязательная категория тикета
    public int CategoryId { get; set; }

    [Required]  // Категория обязательна
    public TicketCategory Category { get; set; } = null!;

    public int? TicketSubCategoryId { get; set; } // Nullable, так как подкатегория может быть не обязательной
    public TicketSubCategory? TicketSubCategory { get; set; }
    public bool IsDeleted { get; set; }
}
