namespace ITSM.Models;

public class TicketSubCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public TicketCategory Category { get; set; }
}