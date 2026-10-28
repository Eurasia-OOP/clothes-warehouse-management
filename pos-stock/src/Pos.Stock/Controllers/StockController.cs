using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Stock.Data;
using Pos.Stock.Models;
using Pos.Stock.Services;

namespace Pos.Stock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IStockService _stock;

    public StockController(ApplicationDbContext db, IStockService stock)
    {
        _db = db;
        _stock = stock;
    }

    /// <summary>Stock movement history, newest first.</summary>
    [HttpGet("movements")]
    public async Task<IEnumerable<StockMovement>> Movements([FromQuery] int? productId)
    {
        var query = _db.StockMovements.AsNoTracking().AsQueryable();
        if (productId.HasValue) query = query.Where(m => m.ProductId == productId);
        return await query.OrderByDescending(m => m.CreatedAt).Take(200).ToListAsync();
    }

    /// <summary>Register purchases, returns or manual adjustments.</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(StockAdjustmentRequest request)
    {
        try
        {
            var movement = await _stock.AdjustAsync(request);
            return Ok(movement);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { error = "Stock was modified by another operation. Try again." });
        }
    }
}
