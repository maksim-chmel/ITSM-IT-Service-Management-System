namespace ITSM.Models;

public class KnowledgeBaseArticle
{
    public int Id { get; set; }
    public string? Article { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }

    public string AuthorId { get; set; }     
    public User Author { get; set; }          

    public int CategoryId { get; set; }        
    public TicketCategory Category { get; set; }
}
