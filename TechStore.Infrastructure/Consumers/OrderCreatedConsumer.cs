using MassTransit;
using Microsoft.Extensions.Logging;
using TechStore.Application.Events;

namespace TechStore.Infrastructure.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation("Successfully received OrderCreatedEvent for Order ID: {OrderId}, Total: {Total}", 
            context.Message.OrderId, context.Message.TotalAmount);
        
        return Task.CompletedTask;
    }
}