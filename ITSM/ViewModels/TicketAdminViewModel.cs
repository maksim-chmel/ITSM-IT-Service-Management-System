namespace ITSM.ViewModels;

public class TicketAdminViewModel : TicketViewModel
{
    public string? Priority { get; set; } = "Normal";
    public string? AssignedTo { get; set; }
    public string? IpAdrr { get; set; }
    public string? Type { get; set; }
    public string? CreatedBy  { get; set; }
    public string? ContactNumber { get; set; }
}