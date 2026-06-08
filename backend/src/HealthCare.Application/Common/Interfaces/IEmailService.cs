namespace HealthCare.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string name, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string name, string token, CancellationToken ct = default);
}