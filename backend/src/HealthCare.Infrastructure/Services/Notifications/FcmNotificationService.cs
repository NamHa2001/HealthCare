using HealthCare.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HealthCare.Infrastructure.Services.Notifications;

public class FcmNotificationService(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<FcmNotificationService> logger) : INotificationService
{
    public async Task SendPushAsync(string fcmToken, string title, string body, CancellationToken ct = default)
    {
        var serverKey = config["Fcm:ServerKey"];
        if (string.IsNullOrEmpty(serverKey))
        {
            logger.LogWarning("FCM ServerKey not configured, skipping push notification");
            return;
        }

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"key={serverKey}");

        var payload = new
        {
            to = fcmToken,
            notification = new { title, body },
            priority = "high"
        };

        var response = await client.PostAsJsonAsync(
            "https://fcm.googleapis.com/fcm/send", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("FCM push failed: {Status} {Content}", response.StatusCode, content);
            throw new HttpRequestException($"FCM push failed: {response.StatusCode}");
        }
    }
}
