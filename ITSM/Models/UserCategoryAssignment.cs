using ITSM.Enums;

namespace ITSM.Models;

public class UserCategoryAssignment
{
    public string  UserId { get; set; }
    public User User { get; set; }

    public int CategoryId { get; set; }
    public TicketCategory TicketCategory { get; set; }
  
}