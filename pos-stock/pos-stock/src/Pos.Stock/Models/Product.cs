using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pos.Stock.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public int? CategoryId { get; set; }

    [JsonIgnore]
    public Category? Category { get; set; }
}
