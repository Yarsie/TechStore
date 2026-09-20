using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStore.Application.DTOs;
using TechStore.Application.Events;
using TechStore.Application.Services;
using TechStore.Domain.Entities;
using TechStore.Domain.Enums;
using TechStore.Infrastructure.Persistence;

namespace TechStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderService(ApplicationDbContext context, ICartService cartService, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _cartService = cartService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> CheckoutAsync(Guid userId)
        {
            var cart = await _cartService.GetCartAsync(userId);

            if (cart.Items == null || cart.Items.Count == 0)
            {
                throw new InvalidOperationException("Cart is empty");
            }

            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = cart.Items.Sum(item => item.UnitPrice * item.Quantity),
                Items = cart.Items.Select(item => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _cartService.ClearCartAsync(userId);

            await _publishEndpoint.Publish(new OrderCreatedEvent(
                order.Id,
                order.UserId,
                order.TotalAmount,
                order.CreatedAt,
                order.Items.Select(x => new OrderItemMessage(x.ProductId, x.Quantity, x.UnitPrice)).ToList()
            ));

            return order.Id;
        }
    }
}