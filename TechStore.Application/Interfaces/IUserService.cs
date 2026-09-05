using TechStore.Application.DTOs;

namespace TechStore.Application.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);
        Task<bool> UpdateUserRoleAsync(Guid id, UpdateUserRoleDto updateRoleDto);
        Task<bool> DeleteUserAsync(Guid id);
    }
}