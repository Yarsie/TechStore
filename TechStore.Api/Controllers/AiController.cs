using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechStore.Application.DTOs;
using TechStore.Application.Interfaces;

namespace TechStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class AiController : ControllerBase
    {
        private readonly IAiDescriptionService _aiDescriptionService;

        public AiController(IAiDescriptionService aiDescriptionService)
        {
            _aiDescriptionService = aiDescriptionService;
        }

        [HttpPost("generate-description")]
        public async Task<ActionResult<GenerateDescriptionResponseDto>> GenerateDescription([FromBody] GenerateDescriptionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _aiDescriptionService.GenerateDescriptionAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while generating the description" });
            }
        }
    }
}
