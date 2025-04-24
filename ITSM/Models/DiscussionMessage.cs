namespace ITSM.Models;

public class DiscussionMessage
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DiscussionId { get; set; }
    public Discussion Discussion { get; set; }
    public string AuthorId { get; set; }
    public User Author { get; set; }
    
}