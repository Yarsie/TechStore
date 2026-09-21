using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using TechStore.Application.DTOs;
using TechStore.Application.Services;

namespace TechStore.Api.Controllers.OData
{
    public class ODataCategoriesController : ODataController
    {
        private readonly ICategoryService _categoryService;

        public ODataCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<CategoryDto>> Get()
        {
            try
            {
                var categories = _categoryService.GetQueryable();
                return Ok(categories);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving categories" });
            }
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public async Task<ActionResult<CategoryDto>> Get(Guid key)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(key);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the category" });
            }
        }
    }
}
