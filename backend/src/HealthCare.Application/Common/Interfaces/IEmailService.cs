namespace HealthCare.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string name, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string name, string token, CancellationToken ct = default);
    Task SendReminderAsync(string toEmail, string title, string body, CancellationToken ct = default);
    Task SendAccountLockedAsync(string toEmail, string name, CancellationToken ct = default);
    Task SendFamilyInviteAsync(string toEmail, string inviterName, string groupName, string token, CancellationToken ct = default);
}