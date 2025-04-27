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
        // Связь Ticket - User (Автор)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Author)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // Предотвращаем удаление пользователя при удалении тикета

        // Связь Ticket - User (Исполнитель)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedUser)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull); // Если назначение исполнителя удаляется, ставим null

        // Связь между TicketCategory и TicketSubCategory
        modelBuilder.Entity<TicketCategory>()
            .HasMany(c => c.SubCategories)  // Одна категория может иметь много подкатегорий
            .WithOne(s => s.Category)  // Каждая подкатегория связана с одной категорией
            .HasForeignKey(s => s.CategoryId)  // Внешний ключ для подкатегории
            .OnDelete(DeleteBehavior.Cascade); // При удалении категории будут удалены и все её подкатегории
        // DiscussionThread → ApplicationUser (Author)
        modelBuilder.Entity<Discussion>()
            .HasOne(t => t.Author)
            .WithMany() // если у пользователя нет списка своих тредов
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ThreadMessage → DiscussionThread
        modelBuilder.Entity<DiscussionMessage>()
            .HasOne(m => m.Discussion)
            .WithMany(t => t.Messages)
            .HasForeignKey(m => m.DiscussionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ThreadMessage → ApplicationUser (Author)
        modelBuilder.Entity<DiscussionMessage>()
            .HasOne(m => m.Author)
            .WithMany() // если у пользователя нет списка своих сообщений
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Discussion>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Threads)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        // Устанавливаем связь многие ко многим между пользователями и категориями
        modelBuilder.Entity<UserCategoryAssignment>()
            .HasKey(uca => new { uca.UserId, uca.CategoryId });

        modelBuilder.Entity<UserCategoryAssignment>()
            .HasOne(uca => uca.User)
            .WithMany(u => u.UserCategoryAssignments)
            .HasForeignKey(uca => uca.UserId);

        modelBuilder.Entity<UserCategoryAssignment>()
            .HasOne(uca => uca.TicketCategory)
            .WithMany(tc => tc.UserCategoryAssignments)
            .HasForeignKey(uca => uca.CategoryId);
        modelBuilder.Entity<TicketCategory>().HasData(
            new TicketCategory { Id = 1, Name = "Техническая поддержка" },
            new TicketCategory { Id = 2, Name = "Сетевые проблемы" },
            new TicketCategory { Id = 3, Name = "Оборудование" },
            new TicketCategory { Id = 4, Name = "Программное обеспечение" }
        );
        // Связь: KnowledgeBaseArticle → User (Автор статьи)
        modelBuilder.Entity<KnowledgeBaseArticle>()
            .HasOne(a => a.Author)
            .WithMany() // У пользователя нет списка всех его статей — если нужен, можно добавить
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // Если пользователя удалить, статья останется

        // Связь: KnowledgeBaseArticle → TicketCategory (Категория статьи)
        modelBuilder.Entity<KnowledgeBaseArticle>()
            .HasOne(a => a.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Cascade); // Если удалить категорию — статьи тоже удалятся (можно поменять на Restrict)
      
    }
}