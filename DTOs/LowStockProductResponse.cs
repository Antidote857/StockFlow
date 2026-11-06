namespace StockFlow.DTOs;

public class LowStockProductResponse
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int QuantityInStock { get; set; }

    public int ReorderLevel { get; set; }

    public decimal UnitPrice { get; set; }

    public int UnitsToReorder =>
        Math.Max(0, ReorderLevel - QuantityInStock);

    public string StockStatus =>
    QuantityInStock == 0 ? "Out of Stock" : "Low Stock";
}