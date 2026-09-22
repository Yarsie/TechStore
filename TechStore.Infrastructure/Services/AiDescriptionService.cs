using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TechStore.Application.DTOs;
using TechStore.Application.Interfaces;

namespace TechStore.Infrastructure.Services
{
    public class AiDescriptionService : IAiDescriptionService
    {
        private const string ModelName = "llama3.2:3b";
        private const string SystemPrompt = 
            "You are a technical copywriter for TechStore. Based on technical specifications, " +
            "generate a concise, professional product summary in English highlighting its primary use case. " +
            "Consider the technical implications of specs: low impedance headphones are for mobile and casual listening, " +
            "high impedance is for studio monitoring and production; high refresh rate IPS panels are for competitive gaming, " +
            "color-accurate OLED panels are for professional creative workflows, etc. " +
            "Output only the final description without introductory phrases.";

        private readonly HttpClient _httpClient;

        public AiDescriptionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GenerateDescriptionResponseDto> GenerateDescriptionAsync(
            GenerateDescriptionRequestDto request, 
            CancellationToken cancellationToken = default)
        {
            var specificationsText = request.Specifications.Count > 0
                ? string.Join(", ", request.Specifications.Select(s => $"{s.Key}: {s.Value}"))
                : "Standard configuration";

            var userPrompt = 
                $"Product: {request.Title}\n" +
                $"Category: {request.CategoryName}\n" +
                $"Specifications: {specificationsText}\n\n" +
                "Write a concise technical summary explaining the intended use of this product.";

            var requestBody = new
            {
                model = ModelName,
                prompt = userPrompt,
                system = SystemPrompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);

            return new GenerateDescriptionResponseDto
            {
                Description = ollamaResponse?.Response.Trim() ?? string.Empty
            };
        }

        private sealed class OllamaResponse
        {
            [JsonPropertyName("response")]
            public string Response { get; set; } = string.Empty;

            [JsonPropertyName("done")]
            public bool Done { get; set; }
        }
    }
}