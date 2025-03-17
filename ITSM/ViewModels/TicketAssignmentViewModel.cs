using ITSM.Models;

namespace ITSM.ViewModels;

public class TicketAssignmentViewModel
{
    public int TicketId { get; set; }
    public string? TicketTitle { get; set; }
    public string? TicketDescription { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public List<User> AvailableUsers { get; set; }
}