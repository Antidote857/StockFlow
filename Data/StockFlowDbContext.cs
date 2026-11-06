
using Microsoft.EntityFrameworkCore;
using StockFlow.Models;

namespace StockFlow.Data;

public class StockFlowDbContext : DbContext
{
    public StockFlowDbContext(DbContextOptions<StockFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<InventoryMovement> InventoryMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasIndex(p => p.SKU)
                .IsUnique();

            entity.Property(p => p.UnitPrice)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(m => m.Notes)
                .HasMaxLength(250);

            entity.HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
