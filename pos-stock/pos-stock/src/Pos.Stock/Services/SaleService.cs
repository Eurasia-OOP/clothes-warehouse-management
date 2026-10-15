using Microsoft.EntityFrameworkCore;
using Pos.Stock.Data;
using Pos.Stock.Models;

namespace Pos.Stock.Services;

public interface ISaleService
{
    Task<Sale> CreateAsync(CreateSaleRequest request);
}

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _db;

    public SaleService(ApplicationDbContext db) => _db = db;

    public async Task<Sale> CreateAsync(CreateSaleRequest request)
    {
        // Merge duplicate lines for the same product.
        var lines = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var ids = lines.Select(l => l.ProductId).ToList();
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var sale = new Sale { PaymentMethod = request.PaymentMethod };
        var movements = new List<StockMovement>();

        foreach (var line in lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                throw new KeyNotFoundException($"Product {line.ProductId} not found.");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product '{product.Name}' is not active.");

            if (product.StockQuantity < line.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}.");

            product.StockQuantity -= line.Quantity;

            sale.Items.Add(new SaleItem
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = product.Price,
                LineTotal = product.Price * line.Quantity
            });

            movements.Add(new StockMovement
            {
                ProductId = product.Id,
                Quantity = -line.Quantity,
                Type = StockMovementType.Sale,
                Reason = "POS sale"
            });
        }

        sale.Total = sale.Items.Sum(i => i.LineTotal);

        _db.Sales.Add(sale);
        _db.StockMovements.AddRange(movements);
        await _db.SaveChangesAsync();

        // Link movements to the sale now that it has an Id.
        foreach (var m in movements) m.SaleId = sale.Id;
        await _db.SaveChangesAsync();

        return sale;
    }
}
