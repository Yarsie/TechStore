using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Application.Services;

namespace TechStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? User.FindFirst("id")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Unable to extract user ID from token");
            }

            return userId;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout()
        {
            try
            {
                var userId = GetCurrentUserId();
                var orderId = await _orderService.CheckoutAsync(userId);
                return Ok(new { OrderId = orderId, Message = "Order placed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing the order" });
            }
        }
    }
}