using TechStore.Application.DTOs;

namespace TechStore.Application.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(Guid userId);
        Task<CartDto> AddItemAsync(Guid userId, AddToCartDto dto);
        Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity);
        Task<CartDto> RemoveItemAsync(Guid userId, Guid productId);
        Task ClearCartAsync(Guid userId);
    }
}
