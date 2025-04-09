using System.ComponentModel.DataAnnotations;
using ITSM.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Create;

public class DiscussionCreateViewModel
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Description { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public string AuthorId { get; set; }
    public User Author { get; set; }
    
    [Required(ErrorMessage = "Please select a category.")]
    public int? CategoryId { get; set; }
    public TicketCategory Category { get; set; } 
    public List<SelectListItem> Categories { get; set; }
}