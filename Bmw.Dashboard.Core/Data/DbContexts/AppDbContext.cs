using Bmw.Dashboard.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bmw.Dashboard.Core.Data.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<UserConfigEntity> UserConfigs => Set<UserConfigEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // If optionsBuilder is NOT configured yet, it means the migration tool is running.
        // We tell it exactly what provider to use right here.
        if (!optionsBuilder.IsConfigured)
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BMWDashboard",
                "telemetry.db"
            );

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserConfigEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.VehicleVin).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired();
        });
    }
}
