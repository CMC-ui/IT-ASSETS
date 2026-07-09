using ItAssets.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ItAssets.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<AssetDocument> AssetDocuments { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure global query filter for soft deletes
        modelBuilder.Entity<Asset>().HasQueryFilter(a => !a.IsDeleted);

        // Configure relationships
        modelBuilder.Entity<AssetDocument>()
            .HasOne(ad => ad.Asset)
            .WithMany(a => a.Documents)
            .HasForeignKey(ad => ad.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Branch)
            .WithMany(b => b.Assets)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Department)
            .WithMany(d => d.Assets)
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.User)
            .WithMany(u => u.Assets)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Branch)
            .WithMany(b => b.Users)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    public override int SaveChanges()
    {
        PreventRootUserDeletion();
        GenerateAuditLogs();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PreventRootUserDeletion();
        GenerateAuditLogs();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PreventRootUserDeletion()
    {
        var entries = ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity.UserName == "Trevor")
            {
                throw new InvalidOperationException("The root user 'Trevor' cannot be deleted.");
            }
        }
    }

    private void GenerateAuditLogs()
    {
        var entries = ChangeTracker.Entries<Asset>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Deleted => "Deleted",
                EntityState.Modified => entry.Entity.IsDeleted ? "Soft Deleted" : "Modified",
                _ => "Unknown"
            };

            AuditLogs.Add(new AuditLog
            {
                AssetId = entry.Entity.Id == 0 ? null : entry.Entity.Id,
                Action = action,
                Timestamp = DateTime.UtcNow
                // UserId could be set here if we inject IHttpContextAccessor, but we'll leave it null for now unless provided explicitly before save
            });
        }
    }
}
