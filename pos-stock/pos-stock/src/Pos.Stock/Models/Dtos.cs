using System.ComponentModel.DataAnnotations;

namespace Pos.Stock.Models;

public record ProductRequest(
    [Required, MaxLength(50)] string Sku,
    [Required, MaxLength(200)] string Name,
    [Range(0, 1_000_000)] decimal Price,
    [Range(0, int.MaxValue)] int ReorderLevel,
    int? CategoryId,
    bool IsActive = true);

public record CategoryRequest([Required, MaxLength(100)] string Name);

public record StockAdjustmentRequest(
    int ProductId,
    int Quantity,
    StockMovementType Type,
    string? Reason);

public record SaleItemRequest(int ProductId, [Range(1, int.MaxValue)] int Quantity);

public record CreateSaleRequest(
    PaymentMethod PaymentMethod,
    [Required, MinLength(1)] List<SaleItemRequest> Items);
