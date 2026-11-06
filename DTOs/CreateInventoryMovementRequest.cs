
using System.ComponentModel.DataAnnotations;
using StockFlow.Models;

namespace StockFlow.DTOs;

public class CreateInventoryMovementRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [EnumDataType(typeof(MovementType))]
    public MovementType Type { get; set; }

    // Negative quantities are permitted for manual adjustments.
    // The controller validates quantities according to movement type.
    public int Quantity { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}
