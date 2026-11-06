
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.DTOs;
using StockFlow.Models;

namespace StockFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly StockFlowDbContext _context;

    public ProductsController(StockFlowDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var products = await _context.Products
            .AsNoTracking()
            .ToListAsync();

        return products.Select(ToProductResponse).ToList();
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return ToProductResponse(product);
    }

    // POST: api/products
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        CreateProductRequest request)
    {
        if (request.UnitPrice <= 0)
        {
            return BadRequest(new
            {
                message = "Unit price must be greater than zero."
            });
        }

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == request.SKU);

        if (existingProduct != null)
        {
            return Conflict(new
            {
                message = "A product with this SKU already exists."
            });
        }

        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            Description = request.Description,
            QuantityInStock = request.QuantityInStock,
            ReorderLevel = request.ReorderLevel,
            UnitPrice = request.UnitPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        var response = ToProductResponse(product);

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            response);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(
        int id,
        UpdateProductRequest request)
    {
        if (request.UnitPrice <= 0)
        {
            return BadRequest(new
            {
                message = "Unit price must be greater than zero."
            });
        }

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        var duplicateSku = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU && p.Id != id);

        if (duplicateSku)
        {
            return Conflict(new
            {
                message = "A product with this SKU already exists."
            });
        }

        existingProduct.SKU = request.SKU;
        existingProduct.Name = request.Name;
        existingProduct.Description = request.Description;
        existingProduct.QuantityInStock = request.QuantityInStock;
        existingProduct.ReorderLevel = request.ReorderLevel;
        existingProduct.UnitPrice = request.UnitPrice;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var hasMovements = await _context.InventoryMovements
            .AnyAsync(m => m.ProductId == id);

        if (hasMovements)
        {
            return Conflict(new
            {
                message = "Cannot delete a product that has inventory movements."
            });
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/products/low-stock
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<LowStockProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LowStockProductResponse>>>
        GetLowStockProducts()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.QuantityInStock <= p.ReorderLevel)
            .OrderBy(p => p.QuantityInStock)
            .ToListAsync();

        var response = products.Select(p => new LowStockProductResponse
        {
            Id = p.Id,
            SKU = p.SKU,
            Name = p.Name,
            QuantityInStock = p.QuantityInStock,
            ReorderLevel = p.ReorderLevel,
            UnitPrice = p.UnitPrice
        }).ToList();

        return Ok(response);
    }

    // Maps the database entity to the API response DTO.
    private static ProductResponse ToProductResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            QuantityInStock = product.QuantityInStock,
            ReorderLevel = product.ReorderLevel,
            UnitPrice = product.UnitPrice,
            CreatedAt = product.CreatedAt
        };
    }
}
