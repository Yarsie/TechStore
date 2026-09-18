using System.Linq;
using TechStore.Application.DTOs;
using TechStore.Application.Services;

namespace TechStore.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ICacheService _cacheService;
        private readonly IProductService _productService;
        private readonly TimeSpan _cartExpiration = TimeSpan.FromDays(14);

        public CartService(ICacheService cacheService, IProductService productService)
        {
            _cacheService = cacheService;
            _productService = productService;
        }

        public async Task<CartDto> GetCartAsync(Guid userId)
        {
            var cacheKey = $"cart:{userId}";
            var cart = await _cacheService.GetAsync<CartDto>(cacheKey);

            if (cart == null)
            {
                cart = new CartDto { UserId = userId };
                await _cacheService.SetAsync(cacheKey, cart, _cartExpiration);
            }

            return cart;
        }

        public async Task<CartDto> AddItemAsync(Guid userId, AddToCartDto dto)
        {
            var product = await _productService.GetByIdAsync(dto.ProductId);
            
            if (product.StockQuantity < dto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {dto.Quantity}");
            }

            var cacheKey = $"cart:{userId}";
            var cart = await _cacheService.GetAsync<CartDto>(cacheKey);

            if (cart == null)
            {
                cart = new CartDto { UserId = userId };
            }

            var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = dto.Quantity
                });
            }

            await _cacheService.SetAsync(cacheKey, cart, _cartExpiration);
            return cart;
        }

        public async Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity)
        {
            var cacheKey = $"cart:{userId}";
            var cart = await _cacheService.GetAsync<CartDto>(cacheKey);

            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart for user {userId} not found");
            }

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null)
            {
                throw new KeyNotFoundException($"Product {productId} not found in cart");
            }

            if (quantity == 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            await _cacheService.SetAsync(cacheKey, cart, _cartExpiration);
            return cart;
        }

        public async Task<CartDto> RemoveItemAsync(Guid userId, Guid productId)
        {
            var cacheKey = $"cart:{userId}";
            var cart = await _cacheService.GetAsync<CartDto>(cacheKey);

            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart for user {userId} not found");
            }

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null)
            {
                throw new KeyNotFoundException($"Product {productId} not found in cart");
            }

            cart.Items.Remove(item);
            await _cacheService.SetAsync(cacheKey, cart, _cartExpiration);
            return cart;
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cacheKey = $"cart:{userId}";
            await _cacheService.RemoveAsync(cacheKey);
        }
    }
}
