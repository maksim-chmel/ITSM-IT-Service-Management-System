using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Manage;

public class EditKnowBaseViewModel
{
    [Required]
    public int Id { get; set; }
    [Required(ErrorMessage = "The 'Article' field is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "The article title must be between 5 and 200 characters.")]
    public string? Article { get; set; }
    [Required(ErrorMessage = "The 'Content' field is required.")]
    [MinLength(10, ErrorMessage = "The content must be at least 10 characters long.")]
    public string? Content { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }
}