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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка внешних ключей

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

        // Связь Ticket - TicketCategory
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Категория не будет удалена при удалении тикета
        modelBuilder.Entity<TicketCategory>().HasData(
            new TicketCategory { Id = 1, Name = "Техническая поддержка" },
            new TicketCategory { Id = 2, Name = "Сетевые проблемы" },
            new TicketCategory { Id = 3, Name = "Оборудование" },
            new TicketCategory { Id = 4, Name = "Программное обеспечение" }
        );
    }
}