namespace ITSM.Models;

public class TicketHistory
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public Ticket? Ticket { get; set; }
}