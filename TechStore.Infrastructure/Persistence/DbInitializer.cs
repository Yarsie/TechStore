using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Persistence.SeedData;

namespace TechStore.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        try
        {
            await context.Database.MigrateAsync();

            if (await context.Products.AnyAsync())
            {
                return;
            }

            var filePath = Path.Combine(AppContext.BaseDirectory, "Persistence", "SeedData", "products.json");
            
            // Якщо шлях у bin не містить файлу, шукаємо у відносній папці проекту
            if (!File.Exists(filePath))
            {
                var candidate = Path.Combine(Directory.GetCurrentDirectory(), "..", "TechStore.Infrastructure", "Persistence", "SeedData", "products.json");
                if (File.Exists(candidate))
                {
                    filePath = candidate;
                }
            }

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Seed file products.json not found at {Path}", filePath);
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var seedProducts = JsonSerializer.Deserialize<List<ProductSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedProducts == null || !seedProducts.Any())
            {
                return;
            }

            // 1. Отримуємо унікальні категорії
            var categoryNames = seedProducts.Select(p => p.CategoryName).Distinct().ToList();
            var categories = new Dictionary<string, Category>();

            foreach (var catName in categoryNames)
            {
                var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == catName);
                if (category == null)
                {
                    category = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = catName
                    };
                    context.Categories.Add(category);
                }
                categories[catName] = category;
            }

            await context.SaveChangesAsync();

            // 2. Створюємо товари
            var products = seedProducts.Select(p => new Product
            {
                Id = Guid.NewGuid(),
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = categories[p.CategoryName].Id,
                AttributesJson = p.Specifications.ToString()
            }).ToList();

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully seeded database with {Count} products and {CatCount} categories", products.Count, categories.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}