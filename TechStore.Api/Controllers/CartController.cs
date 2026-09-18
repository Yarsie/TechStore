using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using TechStore.Application.DTOs;
using TechStore.Application.Services;

namespace TechStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IValidator<AddToCartDto> _addToCartValidator;
        private readonly IValidator<UpdateCartItemQuantityDto> _updateQuantityValidator;

        public CartController(
            ICartService cartService,
            IValidator<AddToCartDto> addToCartValidator,
            IValidator<UpdateCartItemQuantityDto> updateQuantityValidator)
        {
            _cartService = cartService;
            _addToCartValidator = addToCartValidator;
            _updateQuantityValidator = updateQuantityValidator;
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

        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the cart" });
            }
        }

        [HttpPost("items")]
        public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartDto dto)
        {
            var validationResult = await _addToCartValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var userId = GetCurrentUserId();
                var cart = await _cartService.AddItemAsync(userId, dto);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while adding item to cart" });
            }
        }

        [HttpPut("items/{productId}")]
        public async Task<ActionResult<CartDto>> UpdateItemQuantity(Guid productId, [FromBody] UpdateCartItemQuantityDto dto)
        {
            var validationResult = await _updateQuantityValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var userId = GetCurrentUserId();
                var cart = await _cartService.UpdateItemQuantityAsync(userId, productId, dto.Quantity);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while updating item quantity" });
            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<ActionResult<CartDto>> RemoveItem(Guid productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cart = await _cartService.RemoveItemAsync(userId, productId);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while removing item from cart" });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _cartService.ClearCartAsync(userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while clearing the cart" });
            }
        }
    }
}
