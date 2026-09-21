using System.Text.Json;

namespace TechStore.Infrastructure.Persistence.SeedData;

public class ProductSeedDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public JsonElement Specifications { get; set; }
}