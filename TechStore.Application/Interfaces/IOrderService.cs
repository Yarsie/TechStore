namespace TechStore.Application.Services;

public interface IOrderService
{
    Task<Guid> CheckoutAsync(Guid userId);
}