
namespace StockFlow.DTOs;

public class InventoryMovementResponse
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductSKU { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
