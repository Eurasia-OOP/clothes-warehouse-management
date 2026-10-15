using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Stock.Data;
using Pos.Stock.Models;
using Pos.Stock.Services;

namespace Pos.Stock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ISaleService _sales;

    public SalesController(ApplicationDbContext db, ISaleService sales)
    {
        _db = db;
        _sales = sales;
    }

    [HttpGet]
    public async Task<IEnumerable<Sale>> GetAll() =>
        await _db.Sales.AsNoTracking().Include(s => s.Items)
            .OrderByDescending(s => s.CreatedAt).Take(100).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Sale>> GetById(int id)
    {
        var sale = await _db.Sales.AsNoTracking().Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);
        return sale is null ? NotFound() : sale;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleRequest request)
    {
        try
        {
            var sale = await _sales.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { error = "Stock changed during checkout. Please retry." });
        }
    }

    /// <summary>Simple daily report.</summary>
    [HttpGet("report/daily")]
    public async Task<IActionResult> Daily([FromQuery] DateTime? date)
    {
        var day = (date ?? DateTime.UtcNow).Date;
        var start = DateTime.SpecifyKind(day, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var sales = await _db.Sales.AsNoTracking()
            .Where(s => s.CreatedAt >= start && s.CreatedAt < end)
            .ToListAsync();

        return Ok(new
        {
            date = start.ToString("yyyy-MM-dd"),
            salesCount = sales.Count,
            total = sales.Sum(s => s.Total)
        });
    }
}
