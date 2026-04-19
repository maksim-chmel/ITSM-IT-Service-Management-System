using System.ComponentModel.DataAnnotations;
using ITSM.Services.Archive;

namespace ITSM.Models;

public class TicketSubCategory : ISoftDeletable
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Subcategory name is required.")]
    [StringLength(100, ErrorMessage = "Subcategory name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; } 

    [Required(ErrorMessage = "Category is required.")]
    public TicketCategory Category { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
}
