using Microsoft.EntityFrameworkCore;
using Pos.Stock.Data;
using Pos.Stock.Models;

namespace Pos.Stock.Services;

public interface IStockService
{
    Task<StockMovement> AdjustAsync(StockAdjustmentRequest request);
}

public class StockService : IStockService
{
    private readonly ApplicationDbContext _db;

    public StockService(ApplicationDbContext db) => _db = db;

    public async Task<StockMovement> AdjustAsync(StockAdjustmentRequest request)
    {
        if (request.Quantity == 0)
            throw new InvalidOperationException("Quantity cannot be zero.");

        if (request.Type == StockMovementType.Sale)
            throw new InvalidOperationException("Sale movements are created automatically by sales.");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        // Purchases/returns add stock, so force a positive sign for them.
        var signedQty = request.Type is StockMovementType.Purchase or StockMovementType.Return
            ? Math.Abs(request.Quantity)
            : request.Quantity;

        if (product.StockQuantity + signedQty < 0)
            throw new InvalidOperationException("Stock cannot become negative.");

        product.StockQuantity += signedQty;

        var movement = new StockMovement
        {
            ProductId = product.Id,
            Quantity = signedQty,
            Type = request.Type,
            Reason = request.Reason
        };

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        return movement;
    }
}
