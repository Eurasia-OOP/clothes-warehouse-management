using Microsoft.EntityFrameworkCore;
using Pos.Stock.Models;

namespace Pos.Stock.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Price).HasPrecision(12, 2);

            // PostgreSQL xmin used as optimistic concurrency token
            // (prevents two simultaneous sales overselling the same stock).
            e.Property<uint>("xmin").IsRowVersion();

            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.HasOne(m => m.Product)
             .WithMany()
             .HasForeignKey(m => m.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.Property(s => s.Total).HasPrecision(12, 2);
            e.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<SaleItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(12, 2);
            e.Property(i => i.LineTotal).HasPrecision(12, 2);

            e.HasOne<Sale>()
             .WithMany(s => s.Items)
             .HasForeignKey(i => i.SaleId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Product)
             .WithMany()
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
