namespace ITSM.Models;

public class TicketCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Ticket> Tickets { get; set; } = new();

}