using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechStore.Application.DTOs;
using TechStore.Application.Services;

namespace TechStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user" });
            }
        }

        [HttpPut("{id}/role")]
        public async Task<ActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto updateRoleDto)
        {
            try
            {
                var result = await _userService.UpdateUserRoleAsync(id, updateRoleDto);
                if (!result)
                {
                    return NotFound(new { message = "User not found or invalid role" });
                }

                return Ok(new { message = "User role updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user role" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user" });
            }
        }
    }
}