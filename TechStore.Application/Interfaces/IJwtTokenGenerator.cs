using TechStore.Domain.Entities;

namespace TechStore.Application.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}