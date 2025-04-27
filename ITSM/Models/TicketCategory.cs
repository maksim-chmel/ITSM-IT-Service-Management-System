namespace ITSM.Models;

public class TicketCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Ticket> Tickets { get; set; } = new();
    public List<TicketSubCategory> SubCategories { get; set; } = new();
    public List<Discussion> Threads { get; set; } = new();
    public List<UserCategoryAssignment> UserCategoryAssignments { get; set; } = new();
    public List<KnowledgeBaseArticle> Articles { get; set; } = new();
}