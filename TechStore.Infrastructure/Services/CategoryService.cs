using Microsoft.EntityFrameworkCore;
using TechStore.Application.DTOs;
using TechStore.Application.Services;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Persistence;

namespace TechStore.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found");
            }

            return MapToCategoryDto(category);
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _context.Categories
                .ToListAsync();

            return categories.Select(MapToCategoryDto);
        }

        public IQueryable<CategoryDto> GetQueryable()
        {
            return _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                });
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto categoryDto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryDto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToCategoryDto(category);
        }

        public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found");
            }

            category.Name = categoryDto.Name;

            await _context.SaveChangesAsync();

            return MapToCategoryDto(category);
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found");
            }

            if (category.Products.Any())
            {
                throw new InvalidOperationException("Cannot delete category with associated products");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        private static CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}