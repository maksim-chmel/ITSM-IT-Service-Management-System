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

        // Связь между TicketCategory и TicketSubCategory
        modelBuilder.Entity<TicketCategory>()
            .HasMany(c => c.SubCategories)  // Одна категория может иметь много подкатегорий
            .WithOne(s => s.Category)  // Каждая подкатегория связана с одной категорией
            .HasForeignKey(s => s.CategoryId)  // Внешний ключ для подкатегории
            .OnDelete(DeleteBehavior.Cascade); // При удалении категории будут удалены и все её подкатегории
        // Начальные данные для TicketCategory
        modelBuilder.Entity<TicketCategory>().HasData(
            new TicketCategory { Id = 1, Name = "Техническая поддержка" },
            new TicketCategory { Id = 2, Name = "Сетевые проблемы" },
            new TicketCategory { Id = 3, Name = "Оборудование" },
            new TicketCategory { Id = 4, Name = "Программное обеспечение" }
        );
        
      
    }
}