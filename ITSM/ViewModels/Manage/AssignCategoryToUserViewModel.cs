using System.ComponentModel.DataAnnotations;
using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Manage;

public class AssignCategoryToUserViewModel
{
    [Required(ErrorMessage = "UserId is required.")]
    public string UserId { get; set; }

    [Display(Name = "User Name")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Please select at least one category.")]
    [MinLength(1, ErrorMessage = "Please select at least one category.")]
    public List<string> SelectedCategoryIds { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();

    [Required(ErrorMessage = "Skill level is required.")]
    [EnumDataType(typeof(SkillLevel), ErrorMessage = "Invalid skill level.")]
    public SkillLevel SkillLevel { get; set; }
}