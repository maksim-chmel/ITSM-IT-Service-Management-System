using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Create;

public class CreateKnowArtViewModel
{
    [Required(ErrorMessage = "Article title is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Article title must be between 5 and 200 characters.")]
    public string Article { get; set; }

    [Required(ErrorMessage = "Content is required.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 4000 characters.")]
    public string Content { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }
}
