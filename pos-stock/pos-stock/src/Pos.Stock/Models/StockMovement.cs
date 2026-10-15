namespace Pos.Stock.Models;

public enum StockMovementType
{
    Purchase,
    Sale,
    Adjustment,
    Return
}

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Signed quantity: positive = stock in, negative = stock out.</summary>
    public int Quantity { get; set; }

    public StockMovementType Type { get; set; }

    public string? Reason { get; set; }

    public int? SaleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
