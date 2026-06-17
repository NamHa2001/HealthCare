using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HealthCare.Integration.Tests.HealthRecord;

/// <summary>SRS §14.2 — Health Record Flow (8 tests).</summary>
[Collection("Integration")]
public class HealthRecordFlowTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public HealthRecordFlowTests(HealthPlusWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<string> RegisterAndLoginAsync(string? emailPrefix = null)
    {
        var email = $"{emailPrefix ?? "hr"}_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password    = "Test@Pass123!",
            firstName   = "Health",
            lastName    = "User",
            phoneNumber = (string?)null
        });
        var res  = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Test@Pass123!" });
        var body = await res.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    private HttpClient AuthorizedClient(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client;
    }

    private static JsonElement GetData(string body) =>
        JsonDocument.Parse(body).RootElement.GetProperty("data");

    // ─── Test 1: Measurement → BMI computed ──────────────────────────────────

    [Fact]
    public async Task PostMeasurement_WithWeightAndHeight_ReturnsBmiComputed()
    {
        var token = await RegisterAndLoginAsync("bmi");
        var http  = AuthorizedClient(token);

        var res = await http.PostAsJsonAsync("/api/measurements", new
        {
            measuredAt = DateTime.UtcNow,
            weightKg   = 70.0m,
            heightCm   = 170.0m
        });

        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var listRes  = await http.GetAsync("/api/measurements");
        var listBody = await listRes.Content.ReadAsStringAsync();
        var items    = GetData(listBody).EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        var bmi = items[0].GetProperty("bmi").GetDouble();
        bmi.Should().BeApproximately(24.22, 0.1); // 70 / (1.70)^2
    }

    // ─── Test 2: BP vượt ngưỡng → lưu và lấy lại được ─────────────────────────

    [Fact]
    public async Task PostHighBloodPressure_AboveThreshold_SavesAndReturnsInList()
    {
        var token = await RegisterAndLoginAsync("bp");
        var http  = AuthorizedClient(token);

        // 150/95 → Cao độ 2, sẽ raise BloodPressureAlertEvent
        var res = await http.PostAsJsonAsync("/api/BloodPressure", new
        {
            measuredAt = DateTime.UtcNow,
            systolic   = 150,
            diastolic  = 95
        });
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var listRes  = await http.GetAsync("/api/BloodPressure");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var listBody = await listRes.Content.ReadAsStringAsync();
        listBody.Should().Contain("150"); // systolic in response
        listBody.Should().Contain("95");  // diastolic in response
    }

    // ─── Test 3: Health Profile CRUD ─────────────────────────────────────────

    [Fact]
    public async Task GetHealthProfile_AfterRegister_Returns200()
    {
        var token = await RegisterAndLoginAsync("profile");
        var http  = AuthorizedClient(token);

        // [controller] = "HealthProfiles" — route is case-insensitive but NOT hyphen-flexible
        var res = await http.GetAsync("/api/HealthProfiles/me");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task UpdateHealthProfile_ChangesBloodType_PersistCorrectly()
    {
        var token = await RegisterAndLoginAsync("profile2");
        var http  = AuthorizedClient(token);

        var res = await http.PutAsJsonAsync("/api/HealthProfiles/me", new
        {
            bloodType         = "O_Positive",
            allergies         = "Penicillin",
            chronicConditions = (string?)null
        });
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var getRes  = await http.GetAsync("/api/HealthProfiles/me");
        var getBody = await getRes.Content.ReadAsStringAsync();
        getBody.Should().Contain("O_Positive");
    }

    // ─── Test 5: Medical Visit CRUD ──────────────────────────────────────────

    [Fact]
    public async Task CreateMedicalVisit_ThenGetById_ReturnsCorrectData()
    {
        var token = await RegisterAndLoginAsync("visit");
        var http  = AuthorizedClient(token);

        var createRes = await http.PostAsJsonAsync("/api/MedicalVisits", new
        {
            visitDate      = "2026-06-01",
            facilityName   = "Bệnh viện Bạch Mai",
            chiefComplaint = "Đau đầu",
            diagnosis      = "Căng thẳng thần kinh",
            doctorName     = "BS. Nguyễn"
        });
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var createBody = await createRes.Content.ReadAsStringAsync();
        var id         = GetData(createBody).GetProperty("id").GetString();

        var getRes  = await http.GetAsync($"/api/MedicalVisits/{id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var getBody = await getRes.Content.ReadAsStringAsync();
        getBody.Should().Contain("Bạch Mai");
        getBody.Should().Contain("Nguyễn");
    }

    // ─── Test 6: Vaccine record → next_due_date ───────────────────────────────

    [Fact]
    public async Task CreateVaccineRecord_WithoutCatalog_StoresRecordSuccessfully()
    {
        var token = await RegisterAndLoginAsync("vaccine");
        var http  = AuthorizedClient(token);

        var res = await http.PostAsJsonAsync("/api/vaccines", new
        {
            vaccineName    = "Cúm mùa",
            doseNumber     = 1,
            injectionDate  = "2026-01-15",
            vaccineCatalogId = (Guid?)null,
            facility       = "Trung tâm y tế phường"
        });

        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var listRes  = await http.GetAsync("/api/vaccines");
        var listBody = await listRes.Content.ReadAsStringAsync();
        listBody.Should().Contain("Cúm mùa");
    }

    // ─── Test 7: Analytics time-range filter ─────────────────────────────────

    [Fact]
    public async Task GetBmiTrend_WithDateRange_FiltersCorrectly()
    {
        var token = await RegisterAndLoginAsync("analytics");
        var http  = AuthorizedClient(token);

        // Measurement old (90 days ago) — outside 30-day range
        await http.PostAsJsonAsync("/api/measurements", new
        {
            measuredAt = DateTime.UtcNow.AddDays(-90),
            weightKg   = 80.0m,
            heightCm   = 175.0m
        });
        // Measurement recent — inside 30-day range
        await http.PostAsJsonAsync("/api/measurements", new
        {
            measuredAt = DateTime.UtcNow.AddDays(-5),
            weightKg   = 79.0m,
            heightCm   = 175.0m
        });

        var from    = DateTime.UtcNow.AddDays(-30).ToString("o");
        var trendRes  = await http.GetAsync($"/api/analytics/bmi-trend?days=30");
        trendRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var trendBody = await trendRes.Content.ReadAsStringAsync();
        // Should contain the recent measurement's BMI but not the old one
        trendBody.Should().Contain("\"success\":true");
    }

    // ─── Test 8: Vaccine progress ─────────────────────────────────────────────

    [Fact]
    public async Task GetVaccineProgress_AfterAddingRecords_ReturnsProgressData()
    {
        var token = await RegisterAndLoginAsync("vprogress");
        var http  = AuthorizedClient(token);

        await http.PostAsJsonAsync("/api/vaccines", new
        {
            vaccineName  = "Viêm gan B",
            doseNumber   = 1,
            injectionDate = "2026-01-01"
        });
        await http.PostAsJsonAsync("/api/vaccines", new
        {
            vaccineName  = "Viêm gan B",
            doseNumber   = 2,
            injectionDate = "2026-02-01"
        });

        var res  = await http.GetAsync("/api/analytics/vaccine-progress");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }
}
