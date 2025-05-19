using System.ComponentModel.DataAnnotations;
using ITSM.Services.Archive;

namespace ITSM.Models;
public class KnowledgeBaseArticle: ISoftDeletableEntity
{
    public int Id { get; set; }
    public string Article { get; set; } = string.Empty; 
    public string Content { get; set; } = string.Empty;  

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

    [Required]  
    public string AuthorId { get; set; } = string.Empty;

    [Required] 
    public User Author { get; set; } = null!;

    [Required]  
    public int CategoryId { get; set; }

    [Required] 
    public TicketCategory Category { get; set; } = null!;
    public bool IsDeleted { get; set; }= false;
}
