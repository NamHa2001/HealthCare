using FluentAssertions;
using System.Net;

namespace HealthCare.Integration.Tests.Health;

[Collection("Integration")]
public class HealthControllerTests
{
    private readonly HttpClient _client;

    public HealthControllerTests(HealthPlusWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithStatus()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }
}
