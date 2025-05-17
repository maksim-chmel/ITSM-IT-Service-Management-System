using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Manage;

public class CreateKnowArtViewModel
{
    [Required(ErrorMessage = "Article title is required.")]
    public string Article { get; set; }

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }
}
