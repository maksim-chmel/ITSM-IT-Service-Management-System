using ITSM.Enums;

namespace ITSM.ViewModels;

public class TicketListViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public TicketStatus Status { get; set; }
    public string? AuthorName { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ClosedAt { get; set; }
}