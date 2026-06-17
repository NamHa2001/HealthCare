using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace HealthCare.Integration.Tests.Auth;

[Collection("Integration")]
public class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(HealthPlusWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_Returns200()
    {
        var payload = new
        {
            email = $"reg_{Guid.NewGuid():N}@test.com",
            password = "Test@Pass123!",
            firstName = "Integration",
            lastName = "Test",
            phoneNumber = (string?)null
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var payload = new { email, password = "Test@Pass123!", firstName = "A", lastName = "B", phoneNumber = (string?)null };

        await _client.PostAsJsonAsync("/api/auth/register", payload);
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns403()
    {
        var payload = new { email = "nobody@test.com", password = "AnyPass123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_AfterRegister_Returns200WithTokens()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        var registerPayload = new { email, password = "Test@Pass123!", firstName = "User", lastName = "Test", phoneNumber = (string?)null };
        await _client.PostAsJsonAsync("/api/auth/register", registerPayload);

        var loginPayload = new { email, password = "Test@Pass123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("accessToken");
        body.Should().Contain("refreshToken");
    }
}
