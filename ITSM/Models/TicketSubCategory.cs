using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;

public class TicketSubCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Subcategory name is required.")]
    [StringLength(100, ErrorMessage = "Subcategory name cannot exceed 100 characters.")]
    public string Name { get; set; } 

    [Required(ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; } 

    [Required(ErrorMessage = "Category is required.")]
    public TicketCategory Category { get; set; }
    public bool IsDeleted { get; set; } = false;
}
