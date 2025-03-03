using ITSM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITSM.DB;

public class DBaseContext:IdentityDbContext<User>
{
    public DBaseContext(DbContextOptions<DBaseContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
}