using System.ComponentModel.DataAnnotations;

namespace ITSM.ViewModels.Create;

public class SubCategoryCreateViewModel
{
    [Required(ErrorMessage = "Subcategory name is required.")]
    public string Name { get; set; }

    [Required]
    public int CategoryId { get; set; }
}