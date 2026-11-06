using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public enum MovementType
{
    Received,
    Sold,
    Damaged,
    Adjustment
}

public class InventoryMovement
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Required]
    public MovementType Type { get; set; }

    public int Quantity { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}