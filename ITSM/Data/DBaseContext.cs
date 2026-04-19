using ITSM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITSM.Data;

public class DBaseContext(DbContextOptions<DBaseContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketHistory> TicketHistory { get; set; }
    public DbSet<TicketCategory> TicketCategories { get; set; }
    public DbSet<TicketSubCategory> TicketSubCategories { get; set; }
    public DbSet<Discussion> Discussions { get; set; }
    public DbSet<DiscussionMessage> DiscussionMessages { get; set; }
    public DbSet<UserCategoryAssignment> UserCategoryAssignments { get; set; }
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Ticket → Author
    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.Author)
        .WithMany(u => u.CreatedTickets)
        .HasForeignKey(t => t.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    // Ticket → AssignedUser
    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.AssignedUser)
        .WithMany(u => u.AssignedTickets)
        .HasForeignKey(t => t.AssignedUserId)
        .OnDelete(DeleteBehavior.Restrict); 

    // Ticket → TicketSubCategory
    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.TicketSubCategory)
        .WithMany()
        .HasForeignKey(t => t.TicketSubCategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    // TicketSubCategory → TicketCategory
    modelBuilder.Entity<TicketSubCategory>()
        .HasOne(s => s.Category)
        .WithMany(c => c.SubCategories)
        .HasForeignKey(s => s.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    // KnowledgeBaseArticle → TicketCategory
    modelBuilder.Entity<KnowledgeBaseArticle>()
        .HasOne(a => a.Category)
        .WithMany(c => c.Articles)
        .HasForeignKey(a => a.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    // KnowledgeBaseArticle → Author
    modelBuilder.Entity<KnowledgeBaseArticle>()
        .HasOne(a => a.Author)
        .WithMany(u => u.AuthoredArticles)
        .HasForeignKey(a => a.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    // Discussion → Author
    modelBuilder.Entity<Discussion>()
        .HasOne(d => d.Author)
        .WithMany(u => u.AuthoredDiscussions)
        .HasForeignKey(d => d.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    // Discussion → Category
    modelBuilder.Entity<Discussion>()
        .HasOne(d => d.Category)
        .WithMany(c => c.Threads)
        .HasForeignKey(d => d.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    // DiscussionMessage → Discussion
    modelBuilder.Entity<DiscussionMessage>()
        .HasOne(m => m.Discussion)
        .WithMany(d => d.Messages)
        .HasForeignKey(m => m.DiscussionId)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

    // DiscussionMessage → Author
    modelBuilder.Entity<DiscussionMessage>()
        .HasOne(m => m.Author)
        .WithMany()
        .HasForeignKey(m => m.AuthorId)
        .IsRequired(false) // Allow loading message even if author is soft-deleted
        .OnDelete(DeleteBehavior.Restrict);

    // Ticket -> KB Article
    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.KnowledgeBaseArticle)
        .WithMany()
        .HasForeignKey(t => t.KnowledgeBaseArticleId)
        .IsRequired(false)
        .OnDelete(DeleteBehavior.Restrict);
        
    // Discussion -> Ticket
    modelBuilder.Entity<Discussion>()
        .HasOne(d => d.Ticket)
        .WithMany(t => t.Discussions)
        .HasForeignKey(d => d.TicketId)
        .IsRequired(false) // Discussion can exist without a ticket or with an archived one
        .OnDelete(DeleteBehavior.Restrict);

    // TicketHistory -> Ticket
    modelBuilder.Entity<TicketHistory>()
        .HasOne(th => th.Ticket)
        .WithMany(t => t.TicketHistory)
        .HasForeignKey(th => th.TicketId)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

    // UserCategoryAssignment -> User
    modelBuilder.Entity<UserCategoryAssignment>()
        .HasKey(uca => new { uca.UserId, uca.CategoryId });

    modelBuilder.Entity<UserCategoryAssignment>()
        .HasOne(uca => uca.User)
        .WithMany(u => u.UserCategoryAssignments)
        .HasForeignKey(uca => uca.UserId)
        .IsRequired()
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<UserCategoryAssignment>()
        .HasOne(uca => uca.TicketCategory)
        .WithMany(tc => tc.UserCategoryAssignments)
        .HasForeignKey(uca => uca.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

   
    modelBuilder.Entity<TicketCategory>().HasData(
        new TicketCategory { Id = 1, Name = "Technical Support" },
        new TicketCategory { Id = 2, Name = "Network Issues" },
        new TicketCategory { Id = 3, Name = "Hardware" },
        new TicketCategory { Id = 4, Name = "Software" },
        new TicketCategory { Id = 5, Name = "Unknown" }
    );

    // Global Query Filters for Soft Delete
    modelBuilder.Entity<Ticket>().HasQueryFilter(t => !t.IsDeleted);
    modelBuilder.Entity<KnowledgeBaseArticle>().HasQueryFilter(a => !a.IsDeleted);
    modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    modelBuilder.Entity<Discussion>().HasQueryFilter(d => !d.IsDeleted);
    modelBuilder.Entity<UserCategoryAssignment>().HasQueryFilter(uca => !uca.IsDeleted);
    modelBuilder.Entity<DiscussionMessage>().HasQueryFilter(dm => !dm.IsDeleted);
    modelBuilder.Entity<TicketHistory>().HasQueryFilter(th => !th.IsDeleted);
    modelBuilder.Entity<TicketCategory>().HasQueryFilter(tc => !tc.IsDeleted);
    modelBuilder.Entity<TicketSubCategory>().HasQueryFilter(tsc => !tsc.IsDeleted);
}






}
