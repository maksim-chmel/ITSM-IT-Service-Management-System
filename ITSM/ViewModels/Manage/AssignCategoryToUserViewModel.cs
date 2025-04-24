using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Manage;

public class AssignCategoryToUserViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> SelectedCategoryIds { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public SkillLevel SkillLevel { get; set; }
}