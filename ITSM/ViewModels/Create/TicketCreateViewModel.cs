using System.ComponentModel.DataAnnotations;
using ITSM.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSM.ViewModels.Create;

public class TicketCreateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва заявки обов'язкова")]
    [StringLength(100, ErrorMessage = "Назва не може бути довше 100 символів")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Опис заявки обов'язковий")]
    [StringLength(1000, ErrorMessage = "Опис не може бути довше 1000 символів")]
    public string? Description { get; set; }

    [DataType(DataType.DateTime)] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Status Status { get; set; }

    [Required(ErrorMessage = "Категорія обов'язкова")]
    public int? CategoryId { get; set; }
    public string? AuthorId { get; set; }
    public IEnumerable<SelectListItem> Categories { get; set; }
    public string? AuthorName { get; set; }
    public string? CategoryName { get; set; }
  
    public int? SubCategoryId { get; set; }

  
    public List<SelectListItem> SubCategories { get; set; } = new();
}