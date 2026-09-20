namespace TechStore.Application.Events;

public record OrderItemMessage(Guid ProductId, int Quantity, decimal UnitPrice);

public record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<OrderItemMessage> Items
);