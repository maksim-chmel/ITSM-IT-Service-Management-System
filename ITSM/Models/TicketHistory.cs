using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;

public class TicketHistory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "TicketId is required.")]
    public int TicketId { get; set; } // Поле обязательно для связи с Ticket

    public string? AdminComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Добавил значение по умолчанию

    public Ticket Ticket { get; set; } // Убрано Nullable, так как связь обязательная
}
