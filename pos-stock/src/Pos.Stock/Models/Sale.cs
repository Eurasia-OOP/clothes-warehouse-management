namespace Pos.Stock.Models;

public enum PaymentMethod
{
    Cash,
    Card,
    Transfer
}

public class Sale
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Total { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    /// <summary>Price at the moment of sale (price history is preserved).</summary>
    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}
