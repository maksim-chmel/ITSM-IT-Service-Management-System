using ITSM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITSM.DB;

public class DBaseContext:IdentityDbContext<User>
{
    public DBaseContext(DbContextOptions<DBaseContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketHistory> TicketHistory { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка внешнего ключа между Ticket и TicketHistory
        modelBuilder.Entity<TicketHistory>()
            .HasOne(th => th.Ticket)  // Навигационное свойство в TicketHistory
            .WithMany(t => t.TicketHistory)  // Навигационное свойство в Ticket
            .HasForeignKey(th => th.TicketId)  // Внешний ключ в TicketHistory
            .OnDelete(DeleteBehavior.Cascade);  // Удалять историю при удалении тикета
    }
}