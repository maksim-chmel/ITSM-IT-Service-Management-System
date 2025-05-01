using ITSM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITSM.DB;

public class DBaseContext(DbContextOptions<DBaseContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<User> Users { get; set; }
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
        .OnDelete(DeleteBehavior.Restrict); // запретить удаление, если есть связанные заявки

    // Ticket → AssignedUser
    modelBuilder.Entity<Ticket>()
        .HasOne(t => t.AssignedUser)
        .WithMany(u => u.AssignedTickets)
        .HasForeignKey(t => t.AssignedUserId)
        .OnDelete(DeleteBehavior.Restrict); // только вручную разрывать связи

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
        .OnDelete(DeleteBehavior.Restrict); // запретить каскад

    // KnowledgeBaseArticle → TicketCategory
    modelBuilder.Entity<KnowledgeBaseArticle>()
        .HasOne(a => a.Category)
        .WithMany(c => c.Articles)
        .HasForeignKey(a => a.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    // KnowledgeBaseArticle → Author
    modelBuilder.Entity<KnowledgeBaseArticle>()
        .HasOne(a => a.Author)
        .WithMany()
        .HasForeignKey(a => a.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    // Discussion → Author
    modelBuilder.Entity<Discussion>()
        .HasOne(d => d.Author)
        .WithMany()
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
        .OnDelete(DeleteBehavior.Restrict);

    // DiscussionMessage → Author
    modelBuilder.Entity<DiscussionMessage>()
        .HasOne(m => m.Author)
        .WithMany()
        .HasForeignKey(m => m.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    // UserCategoryAssignment → User / TicketCategory
    modelBuilder.Entity<UserCategoryAssignment>()
        .HasKey(uca => new { uca.UserId, uca.CategoryId });

    modelBuilder.Entity<UserCategoryAssignment>()
        .HasOne(uca => uca.User)
        .WithMany(u => u.UserCategoryAssignments)
        .HasForeignKey(uca => uca.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<UserCategoryAssignment>()
        .HasOne(uca => uca.TicketCategory)
        .WithMany(tc => tc.UserCategoryAssignments)
        .HasForeignKey(uca => uca.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    // Начальные данные категорий
    modelBuilder.Entity<TicketCategory>().HasData(
        new TicketCategory { Id = 1, Name = "Техническая поддержка" },
        new TicketCategory { Id = 2, Name = "Сетевые проблемы" },
        new TicketCategory { Id = 3, Name = "Оборудование" },
        new TicketCategory { Id = 4, Name = "Программное обеспечение" },
        new TicketCategory { Id = 5, Name = "Unknown" }
    );

  
}






}