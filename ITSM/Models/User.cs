using Microsoft.AspNetCore.Identity;

namespace ITSM.Models;

public class User : IdentityUser
{
    public string? Role { get; set; }
    // Все заявки, созданные пользователем
    public List<Ticket> CreatedTickets { get; set; } = new();

    // Все заявки, назначенные этому пользователю на исполнение
    public List<Ticket> AssignedTickets { get; set; } = new();
}