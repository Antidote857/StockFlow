
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.DTOs;
using StockFlow.Models;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryMovementsController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public InventoryMovementsController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/inventorymovements
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InventoryMovementResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventoryMovementResponse>>>
        GetMovements()
    {
        var movements = await _context.InventoryMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(movements.Select(ToMovementResponse).ToList());
    }

    // GET: api/inventorymovements/5
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InventoryMovementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryMovementResponse>>
        GetMovement(int id)
    {
        var movement = await _context.InventoryMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movement == null)
        {
            return NotFound(new
            {
                message = "Inventory movement not found."
            });
        }

        return Ok(ToMovementResponse(movement));
    }

    // GET: api/inventorymovements/product/5
    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(IEnumerable<InventoryMovementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<InventoryMovementResponse>>>
        GetProductMovements(int productId)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == productId);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var movements = await _context.InventoryMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(movements.Select(ToMovementResponse).ToList());
    }

    // POST: api/inventorymovements
    [HttpPost]
    [ProducesResponseType(typeof(InventoryMovementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryMovementResponse>>
        CreateMovement(CreateInventoryMovementRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        if (!Enum.IsDefined(typeof(MovementType), request.Type))
        {
            return BadRequest(new
            {
                message = "Invalid inventory movement type."
            });
        }

        long stockChange;

        switch (request.Type)
        {
            case MovementType.Received:
                if (request.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Received quantity must be greater than zero."
                    });
                }

                stockChange = request.Quantity;
                break;

            case MovementType.Sold:
                if (request.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Sold quantity must be greater than zero."
                    });
                }

                stockChange = -(long)request.Quantity;
                break;

            case MovementType.Damaged:
                if (request.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Damaged quantity must be greater than zero."
                    });
                }

                stockChange = -(long)request.Quantity;
                break;

            case MovementType.Adjustment:
                if (request.Quantity == 0)
                {
                    return BadRequest(new
                    {
                        message = "Adjustment quantity cannot be zero."
                    });
                }

                stockChange = request.Quantity;
                break;

            default:
                return BadRequest(new
                {
                    message = "Invalid inventory movement type."
                });
        }

        long updatedStock =
            (long)product.QuantityInStock + stockChange;

        if (updatedStock < 0)
        {
            return BadRequest(new
            {
                message = "Insufficient stock for this movement.",
                availableStock = product.QuantityInStock
            });
        }

        if (updatedStock > int.MaxValue)
        {
            return BadRequest(new
            {
                message = "The resulting stock exceeds the supported quantity."
            });
        }

        product.QuantityInStock = (int)updatedStock;

        var movement = new InventoryMovement
        {
            ProductId = request.ProductId,
            Product = product,
            Type = request.Type,
            Quantity = request.Quantity,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryMovements.Add(movement);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMovement),
            new { id = movement.Id },
            ToMovementResponse(movement));
    }

    // Converts the database entity into an API response DTO.
    private static InventoryMovementResponse ToMovementResponse(
        InventoryMovement movement)
    {
        return new InventoryMovementResponse
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = movement.Product?.Name ?? string.Empty,
            ProductSKU = movement.Product?.SKU ?? string.Empty,
            Type = movement.Type.ToString(),
            Quantity = movement.Quantity,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt
        };
    }
}
