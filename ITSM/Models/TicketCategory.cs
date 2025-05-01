using System.ComponentModel.DataAnnotations;

namespace ITSM.Models;
public class TicketCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty; // Присваиваем значение по умолчанию (пустая строка)

    public List<Ticket> Tickets { get; set; } = new();
    public List<TicketSubCategory> SubCategories { get; set; } = new();
    public List<Discussion> Threads { get; set; } = new();
    public List<UserCategoryAssignment> UserCategoryAssignments { get; set; } = new();
    public List<KnowledgeBaseArticle> Articles { get; set; } = new();
    public bool IsDeleted { get; set; } = false;
}
