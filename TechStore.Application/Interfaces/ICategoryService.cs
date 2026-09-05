using TechStore.Application.DTOs;

namespace TechStore.Application.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto> GetByIdAsync(Guid id);
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        IQueryable<CategoryDto> GetQueryable();
        Task<CategoryDto> CreateAsync(CreateCategoryDto categoryDto);
        Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto categoryDto);
        Task DeleteAsync(Guid id);
    }
}