using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Create;

public class DiscussionMessageCreateViewModel
{
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    public string MessageContent { get; set; } = string.Empty;
}
