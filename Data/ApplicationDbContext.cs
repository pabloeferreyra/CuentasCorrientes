using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CuentasCorrientes.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Models.Client> Clients { get; set; } = null!;
    public DbSet<Models.ClientType> ClientTypes { get; set; } = null!;
    public DbSet<Models.CurrentAccounts> CurrentAccounts { get; set; } = null!;
    public DbSet<Models.Movements> Movements { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Models.Client>().ToTable("Clients");
        modelBuilder.Entity<Models.ClientType>().ToTable("ClientTypes");
        modelBuilder.Entity<Models.CurrentAccounts>().ToTable("CurrentAccounts");
        modelBuilder.Entity<Models.Movements>().ToTable("Movements");
        modelBuilder.Entity<Models.Client>()
            .HasOne(c => c.ClientType)
            .WithMany(ct => ct.Clients)
            .HasForeignKey(c => c.ClientTypeId);
    }
}
