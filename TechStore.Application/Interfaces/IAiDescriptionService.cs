using TechStore.Application.DTOs;

namespace TechStore.Application.Interfaces
{
    public interface IAiDescriptionService
    {
        Task<GenerateDescriptionResponseDto> GenerateDescriptionAsync(GenerateDescriptionRequestDto request, CancellationToken cancellationToken = default);
    }
}
