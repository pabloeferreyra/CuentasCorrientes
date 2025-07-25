using CuentasCorrientes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CuentasCorrientes.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : IdentityDbContext(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<ClientType> ClientTypes { get; set; } = null!;
    public DbSet<CurrentAccounts> CurrentAccounts { get; set; } = null!;
    public DbSet<Movements> Movements { get; set; } = null!;

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        foreach (var entry in ChangeTracker.Entries<Movements>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedByUserId = userId;
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedByUserId = userId;
                entry.Entity.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Client>().ToTable("Clients");
        modelBuilder.Entity<ClientType>().ToTable("ClientTypes");
        modelBuilder.Entity<CurrentAccounts>().ToTable("CurrentAccounts");
        modelBuilder.Entity<Movements>().ToTable("Movements");
        modelBuilder.Entity<Client>()
            .HasOne(c => c.ClientType)
            .WithMany(ct => ct.Clients)
            .HasForeignKey(c => c.ClientTypeId);
    }
}
