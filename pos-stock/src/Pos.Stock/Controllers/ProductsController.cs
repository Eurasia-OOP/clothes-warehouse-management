using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Stock.Data;
using Pos.Stock.Models;

namespace Pos.Stock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Product>> GetAll(
        [FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%")
                                  || EF.Functions.ILike(p.Sku, $"%{search}%"));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    [HttpGet("low-stock")]
    public async Task<IEnumerable<Product>> LowStock() =>
        await _db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity <= p.ReorderLevel)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductRequest request)
    {
        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku))
            return Conflict(new { error = $"SKU '{request.Sku}' already exists." });

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Price = request.Price,
            ReorderLevel = request.ReorderLevel,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku && p.Id != id))
            return Conflict(new { error = $"SKU '{request.Sku}' already exists." });

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Price = request.Price;
        product.ReorderLevel = request.ReorderLevel;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Soft delete: products with history cannot be removed, only deactivated.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
