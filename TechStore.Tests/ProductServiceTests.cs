using Microsoft.EntityFrameworkCore;
using Moq;
using TechStore.Application.DTOs;
using TechStore.Application.Services;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Persistence;
using TechStore.Infrastructure.Services;
using Xunit;

namespace TechStore.Tests;

public class ProductServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly ProductService _productService;
    private readonly Guid _categoryId;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _mockCacheService = new Mock<ICacheService>();
        _mockCacheService
            .Setup(x => x.GetAsync<ProductResponseDto>(It.IsAny<string>()))
            .ReturnsAsync((ProductResponseDto?)null);

        _categoryId = Guid.NewGuid();
        _context.Categories.Add(new Category { Id = _categoryId, Name = "Electronics" });
        _context.SaveChanges();

        _productService = new ProductService(_context, _mockCacheService.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 999.99m,
            StockQuantity = 10,
            AttributesJson = "{}",
            CategoryId = _categoryId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal(999.99m, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _productService.GetByIdAsync(nonExistingId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenFoundInCache_ReturnsCachedProductWithoutDbQuery()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cachedProduct = new ProductResponseDto
        {
            Id = productId,
            Name = "Cached Headset",
            Price = 120.00m
        };

        _mockCacheService
            .Setup(x => x.GetAsync<ProductResponseDto>($"products:{productId}"))
            .ReturnsAsync(cachedProduct);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cached Headset", result.Name);
        Assert.Equal(120.00m, result.Price);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_SavesToDatabase()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Smartphone",
            Description = "Latest model smartphone",
            Price = 799.99m,
            StockQuantity = 20,
            AttributesJson = "{}",
            CategoryId = _categoryId
        };

        // Act
        var result = await _productService.CreateAsync(createProductDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Smartphone", result.Name);

        var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.Name == "Smartphone");
        Assert.NotNull(productInDb);
        Assert.Equal(799.99m, productInDb.Price);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesDatabaseRecord()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Old Monitor",
            Description = "Old specs",
            Price = 200.00m,
            StockQuantity = 5,
            AttributesJson = "{}",
            CategoryId = _categoryId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Monitor 144Hz",
            Description = "Updated specs",
            Price = 250.00m,
            StockQuantity = 8,
            AttributesJson = "{}",
            CategoryId = _categoryId
        };

        // Act
        await _productService.UpdateAsync(productId, updateDto);

        // Assert
        var updatedInDb = await _context.Products.FindAsync(productId);
        Assert.NotNull(updatedInDb);
        Assert.Equal("Updated Monitor 144Hz", updatedInDb.Name);
        Assert.Equal(250.00m, updatedInDb.Price);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_RemovesFromDatabase()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Item To Delete",
            Description = "Desc",
            Price = 50.00m,
            StockQuantity = 1,
            AttributesJson = "{}",
            CategoryId = _categoryId
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        await _productService.DeleteAsync(productId);

        // Assert
        var productInDb = await _context.Products.FindAsync(productId);
        Assert.Null(productInDb);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}