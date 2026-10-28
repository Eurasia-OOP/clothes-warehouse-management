namespace Pos.Stock.Client.Models;

public sealed class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CategoryForm
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public int? CategoryId { get; set; }
}

public sealed class ProductForm
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ReorderLevel { get; set; }
    public int? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int? SaleId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class StockAdjustmentForm
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string Type { get; set; } = "Purchase";
    public string? Reason { get; set; }
}

public sealed class SaleItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class SaleDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}

public sealed class SaleItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class SaleRequest
{
    public string PaymentMethod { get; set; } = "Cash";
    public List<SaleItemRequest> Items { get; set; } = new();
}

public sealed class DailyReportDto
{
    public string Date { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Total { get; set; }
}
