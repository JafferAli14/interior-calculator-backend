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

    public DbSet<ProjectEstimateLine> ProjectEstimateLines { get; set; }

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

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => p.ProjectNumber)
                .IsUnique();

            entity.Property(p => p.ProjectNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(p => p.ProjectName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.CustomerPhone)
                .HasMaxLength(20);

            entity.Property(p => p.CustomerEmail)
                .HasMaxLength(150);

            entity.Property(p => p.CustomerAddress)
                .HasMaxLength(200);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(p => p.GrandTotal)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.PlannerRequestJson)
                .IsRequired()
                .HasColumnType("longtext");

            entity.Property(p => p.CategorySubtotalsJson)
                .IsRequired()
                .HasColumnType("longtext");

            entity.Property(p => p.WarningsJson)
                .IsRequired()
                .HasColumnType("longtext");

            entity.Property(p => p.CreatedByUsername)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.CreatedByFullName)
                .HasMaxLength(100);

            entity.HasMany(p => p.EstimateLines)
                .WithOne(l => l.Project)
                .HasForeignKey(l => l.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectEstimateLine>(entity =>
        {
            entity.Property(l => l.PriceItemCode)
                .HasMaxLength(50);

            entity.Property(l => l.ItemName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(l => l.Category)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(l => l.PricingMode)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(l => l.Selection)
                .HasMaxLength(200);

            entity.Property(l => l.Area)
                .HasColumnType("decimal(18,2)");

            entity.Property(l => l.Length)
                .HasColumnType("decimal(18,2)");

            entity.Property(l => l.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(l => l.Rate)
                .HasColumnType("decimal(18,2)");

            entity.Property(l => l.CustomPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(l => l.Calculation)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(l => l.Amount)
                .HasColumnType("decimal(18,2)");
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
