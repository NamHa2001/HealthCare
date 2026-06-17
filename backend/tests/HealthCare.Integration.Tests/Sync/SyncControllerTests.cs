using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HealthCare.Integration.Tests.Sync;

[Collection("Integration")]
public class SyncControllerTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public SyncControllerTests(HealthPlusWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"sync_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password    = "Test@Pass123!",
            firstName   = "Sync",
            lastName    = "User",
            phoneNumber = (string?)null
        });

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Test@Pass123!"
        });
        var body = await loginRes.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    private static object MakeMeasurementOp(string? id = null) => new
    {
        operation       = "create",
        entityType      = "health_measurement",
        entityId        = id ?? Guid.NewGuid().ToString(),
        payload         = new { weightKg = 70.0, heightCm = 170.0, measuredAt = DateTime.UtcNow },
        clientVersion   = 1L,
        clientTimestamp = DateTime.UtcNow
    };

    private static object MakeBpOp() => new
    {
        operation       = "create",
        entityType      = "blood_pressure",
        entityId        = Guid.NewGuid().ToString(),
        payload         = new { systolic = 120, diastolic = 80, measuredAt = DateTime.UtcNow },
        clientVersion   = 1L,
        clientTimestamp = DateTime.UtcNow
    };

    // ─── Tests ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Pull_Unauthenticated_Returns401()
    {
        var client = _client;
        client.DefaultRequestHeaders.Authorization = null;
        var res = await client.GetAsync("/api/sync/pull?since=0");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Pull_NewUser_ReturnsEmptyChanges()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res  = await _client.GetAsync("/api/sync/pull?since=0");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("changes").GetArrayLength().Should().Be(0);
        data.GetProperty("serverTimestamp").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Push_ValidHealthMeasurement_Returns200()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await _client.PostAsJsonAsync("/api/sync/push",
            new { operations = new[] { MakeMeasurementOp() } });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Push_Then_Pull_ReturnsMeasurementInChanges()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sinceMs = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds();

        await _client.PostAsJsonAsync("/api/sync/push",
            new { operations = new[] { MakeMeasurementOp() } });

        var res     = await _client.GetAsync($"/api/sync/pull?since={sinceMs}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body    = await res.Content.ReadAsStringAsync();
        var doc     = JsonDocument.Parse(body);
        var changes = doc.RootElement.GetProperty("data").GetProperty("changes");
        changes.GetArrayLength().Should().BeGreaterThan(0);
        changes[0].GetProperty("entityType").GetString().Should().Be("health_measurement");
    }

    [Fact]
    public async Task Push_BloodPressureOperation_Returns200()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await _client.PostAsJsonAsync("/api/sync/push",
            new { operations = new[] { MakeBpOp() } });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pull_WithFutureTimestamp_ReturnsEmptyChanges()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/sync/push",
            new { operations = new[] { MakeMeasurementOp() } });

        var future = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeMilliseconds();
        var res    = await _client.GetAsync($"/api/sync/pull?since={future}");
        var body   = await res.Content.ReadAsStringAsync();
        var doc    = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("data").GetProperty("changes").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Push_BatchOfTwoOperations_BothApplied()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sinceMs = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds();

        var pushRes = await _client.PostAsJsonAsync("/api/sync/push", new
        {
            operations = new[] { MakeMeasurementOp(), MakeBpOp() }
        });
        pushRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var pullRes = await _client.GetAsync($"/api/sync/pull?since={sinceMs}");
        var body    = await pullRes.Content.ReadAsStringAsync();
        var doc     = JsonDocument.Parse(body);
        var changes = doc.RootElement.GetProperty("data").GetProperty("changes");
        changes.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }
}
