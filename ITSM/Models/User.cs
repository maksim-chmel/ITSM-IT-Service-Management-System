using Microsoft.AspNetCore.Identity;

namespace ITSM.Models;

public class User : IdentityUser
{
    public string? Role { get; set; }
   
    public List<Ticket> CreatedTickets { get; set; } = new();
    
    public List<Ticket> AssignedTickets { get; set; } = new();
}