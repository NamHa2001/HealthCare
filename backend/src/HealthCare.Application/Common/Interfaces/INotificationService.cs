namespace HealthCare.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendPushAsync(string fcmToken, string title, string body, CancellationToken ct = default);
}
