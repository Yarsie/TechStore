using Microsoft.EntityFrameworkCore;
using TechStore.Application.DTOs;
using TechStore.Application.Services;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Persistence;

namespace TechStore.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            return MapToProductResponseDto(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return products.Select(MapToProductResponseDto);
        }

        public IQueryable<ProductResponseDto> GetQueryable()
        {
            return _context.Products
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    AttributesJson = p.AttributesJson,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name
                    } : null
                });
        }

        public async Task<ProductResponseDto> CreateAsync(CreateProductDto productDto)
        {
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {productDto.CategoryId} not found");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                AttributesJson = productDto.AttributesJson,
                CategoryId = productDto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(product.Id);
        }

        public async Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {productDto.CategoryId} not found");
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.StockQuantity = productDto.StockQuantity;
            product.AttributesJson = productDto.AttributesJson;
            product.CategoryId = productDto.CategoryId;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(product.Id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        private static ProductResponseDto MapToProductResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                AttributesJson = product.AttributesJson,
                CategoryId = product.CategoryId,
                Category = product.Category != null ? new CategoryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name
                } : null
            };
        }
    }
}