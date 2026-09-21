using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using TechStore.Application.DTOs;
using TechStore.Application.Services;

namespace TechStore.Api.Controllers.OData
{
    public class ODataProductsController : ODataController
    {
        private readonly IProductService _productService;

        public ODataProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<ProductResponseDto>> Get()
        {
            try
            {
                var products = _productService.GetQueryable();
                return Ok(products);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving products" });
            }
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public async Task<ActionResult<ProductResponseDto>> Get(Guid key)
        {
            try
            {
                var product = await _productService.GetByIdAsync(key);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the product" });
            }
        }
    }
}
