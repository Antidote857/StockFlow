namespace StockFlow.DTOs;

public class ProductResponse
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int QuantityInStock { get; set; }

    public int ReorderLevel { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }
}