using System.ComponentModel.DataAnnotations;
using ITSM.Enums;
using ITSM.Services.Archive;

namespace ITSM.Models;

public class Ticket: ISoftDeletable
{
    public int Id { get; set; }

   
    public string Title { get; set; } = string.Empty; 

   
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public string? FixDescription { get; set; } 

    [Required]
    public Status Status { get; set; } = Status.New;

    [Required] 
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public string? CancelReason { get; set; } 

  
    public string AuthorId { get; set; } = string.Empty;

   
    public User Author { get; set; } = null!;

    public string? AssignedUserId { get; set; } 
    public User? AssignedUser { get; set; } 

    public List<TicketHistory> TicketHistory { get; set; } = new();

   
    public int CategoryId { get; set; }

    
    public TicketCategory Category { get; set; } = null!;

    public int? TicketSubCategoryId { get; set; } 
    public TicketSubCategory? TicketSubCategory { get; set; }
    
    public int? KnowledgeBaseArticleId { get; set; }
    public KnowledgeBaseArticle? KnowledgeBaseArticle { get; set; }

    public bool IsDeleted { get; set; }
    public List<Discussion> Discussions { get; set; } = new();
}
