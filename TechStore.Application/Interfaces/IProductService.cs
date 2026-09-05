using TechStore.Application.DTOs;

namespace TechStore.Application.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductResponseDto>> GetAllAsync();
        IQueryable<ProductResponseDto> GetQueryable();
        Task<ProductResponseDto> CreateAsync(CreateProductDto productDto);
        Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductDto productDto);
        Task DeleteAsync(Guid id);
    }
}