using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;

public class UserCategoryAssignment
{
    public string UserId { get; set; }

    [Required(ErrorMessage = "User is required.")]
    public User User { get; set; } 

    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public TicketCategory TicketCategory { get; set; } 
}
