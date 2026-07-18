using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<PriceItem> PriceItems { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.Property(a => a.Role)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<PriceItem>(entity =>
        {
            entity.HasIndex(p => p.Code)
                .IsUnique();

            entity.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Category)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Rate)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.VariableType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(a => a.ActorUsername)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(a => a.ActorFullName)
                .HasMaxLength(100);

            entity.Property(a => a.ActorRole)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.EntityCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(a => a.OldValuesJson)
                .IsRequired()
                .HasColumnType("longtext");

            entity.Property(a => a.NewValuesJson)
                .IsRequired()
                .HasColumnType("longtext");
        });
    }
}
