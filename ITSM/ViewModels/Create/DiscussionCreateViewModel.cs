using System.ComponentModel.DataAnnotations;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Create;

public class DiscussionCreateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string? Title { get; set; }
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public string AuthorId { get; set; }
    public User Author { get; set; }
    [Required(ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }
    public TicketCategory Category { get; set; } 
    public List<SelectListItem> Categories { get; set; }
}