using System.Net;
using System.Net.Http;
using System.Text;
using Moq;
using Moq.Protected;
using TechStore.Application.DTOs;
using TechStore.Infrastructure.Services;

namespace TechStore.Tests;

public class AiDescriptionServiceTests
{
    [Fact]
    public async Task GenerateDescription_SuccessfulResponse_ReturnsText()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"response\": \"Great gaming headphones\", \"done\": true}", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var aiDescriptionService = new AiDescriptionService(httpClient);

        var request = new GenerateDescriptionRequestDto
        {
            Title = "Gaming Headphones",
            CategoryName = "Audio",
            Specifications = new Dictionary<string, string>
            {
                { "Type", "Over-ear" },
                { "Impedance", "32 Ohm" }
            }
        };

        // Act
        var result = await aiDescriptionService.GenerateDescriptionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great gaming headphones", result.Description);
    }
}
