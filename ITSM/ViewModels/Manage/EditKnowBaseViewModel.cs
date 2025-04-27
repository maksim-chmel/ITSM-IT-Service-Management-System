using ITSM.Models;

namespace ITSM.ViewModels.Manage;

public class EditKnowBaseViewModel
{
    public int Id { get; set; }
    public string? Article { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }
}