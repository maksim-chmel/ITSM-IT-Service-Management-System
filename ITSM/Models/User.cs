using ITSM.Enums;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Models;

public class User : IdentityUser, ISoftDeletable
{
    public List<Ticket> CreatedTickets { get; set; } = new();
    public List<Ticket> AssignedTickets { get; set; } = new();
    public List<KnowledgeBaseArticle> AuthoredArticles { get; set; } = new();
    public List<Discussion> AuthoredDiscussions { get; set; } = new();
    public List<UserCategoryAssignment> UserCategoryAssignments { get; set; } = new();
    public SkillLevel SkillLevel { get; set; }
    public bool IsDeleted { get; set; } = false;
}