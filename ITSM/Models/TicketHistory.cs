using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;

public class TicketHistory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "TicketId is required.")]
    public int TicketId { get; set; }

    public string? AdminComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; }
}
