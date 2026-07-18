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
    }
}
