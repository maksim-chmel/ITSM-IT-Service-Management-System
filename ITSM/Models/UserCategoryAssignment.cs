using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;

public class UserCategoryAssignment : ISoftDeletable
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "User is required.")]
    public User User { get; set; } = null!;

    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public TicketCategory TicketCategory { get; set; } = null!;
    
    public bool IsDeleted { get; set; }
}
