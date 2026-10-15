using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Stock.Data;
using Pos.Stock.Models;

namespace Pos.Stock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<object>> GetAll() =>
        await _db.Categories.OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name }).ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == request.Name))
            return Conflict(new { error = "Category already exists." });

        var category = new Category { Name = request.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { category.Id, category.Name });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        category.Name = request.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
