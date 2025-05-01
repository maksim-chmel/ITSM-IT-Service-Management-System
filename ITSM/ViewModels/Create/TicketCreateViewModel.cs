using System.ComponentModel.DataAnnotations;
using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Create;

public class TicketCreateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    [DataType(DataType.DateTime)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Status Status { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }
    public string? AuthorId { get; set; }
    public IEnumerable<SelectListItem> Categories { get; set; }
    public string? AuthorName { get; set; }
    public string? CategoryName { get; set; }
    [Required(ErrorMessage = "SubCategory is required.")]
  
    public int? SubCategoryId { get; set; }

  
    public List<SelectListItem> SubCategories { get; set; } = new();
}